using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using DitzelGames.FastIK;
using System.Linq;
using Sandblast.Extensions;

namespace Sandblast
{
    public class Polish : Instrument, IDragHandler, IInitializePotentialDragHandler
    {
        [SerializeField] private Mesh _sphereMesh;
        [SerializeField] private Material _meshMaterial;
        [SerializeField] private Shader _uvShader;
        [SerializeField] private Shader _setupShader;
        [SerializeField] private Shader _fillShader;
        [SerializeField] private Shader _fillColorShader;
        [SerializeField] private Shader _fixIlsandEdgesShader;

        private Mesh _spawnMesh;
        private Vector3[] _vertices;
        private ILookup<Vector3, int> _triangles;
        private HashSet<int> _activeVertices = new HashSet<int>();

        private Camera _camera;

        private RenderTexture _startTex;
        private CommandBuffer _startTexBuffer;

        private PaintableTexture _albedo;
        private bool _inited = false;

        private Ray _ray = new Ray();
        private readonly RaycastHit[] _hits = new RaycastHit[1];

        private void Update()
        {
            if (!_inited)
            {
                return;
            }

            _albedo.UpdateShaderParameters(Target.transform.localToWorldMatrix, UVMask);

            FilledColor.SetBaseTexture(_startTex);
            FilledColor.SetTexture(_albedo.RuntimeTexture);
            FilledColor.SetTargetColor(Color.white);
            FilledColor.SetUVMask(UVMask);
            if (UVMask != null)
            {
                FilledColor.SetUVMask(UVMask);
            }
            else
            {
                FilledColor.SetUVMask(_startTex);
            }

            Shader.SetGlobalColor("_BrushColor", Color.white);
            Shader.SetGlobalFloat("_BrushSize", 0.175f);
            Shader.SetGlobalFloat("_BrushHardness", 0.75f);

            if (Input.GetMouseButtonUp(0) && FilledColor.IsFilled() && !IsCompleted)
            {
                FilledColor.DisableParticles();

                StartCoroutine(_albedo.BlitWithTexture(_startTex, _setupShader, Color.white, UVMask, true));
                InvokeCompletedEvent();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            const float rotationThreshold = 1.25f;

            var coord = _camera.ScreenToWorldPoint((Vector3)eventData.position + Vector3.forward * 1.45f);
            transform.position = Vector3.Slerp(transform.position, coord, 20 * Time.deltaTime);

            _ray.origin = transform.position;
            _ray.direction = Quaternion.Euler(Random.Range(-rotationThreshold, rotationThreshold) * 2, Random.Range(-rotationThreshold, rotationThreshold), 0) * (coord - _camera.transform.position);
            var point = Vector4.one * 999;

            if (Physics.RaycastNonAlloc(_ray, _hits) > 0)
            {
                if (_hits[0].collider.gameObject == Target.gameObject)
                {
                    point = _hits[0].point;

                    var rotation = Quaternion.Inverse(Target.transform.rotation);
                    var scale = VectorExtensions.Divide(Vector3.one, Target.transform.localScale);

                    var minDist = float.MaxValue;
                    var minVertIndex = -1;
                    for (int i = 0; i < _sphereMesh.vertexCount; i++)
                    {
                        //Debug.DrawRay(_sphereMesh.vertices[i], _sphereMesh.normals[i] * 0.1f, Color.red, 10000);
                        //Debug.DrawRay(point, _hits[0].normal * 0.1f, Color.green, 10000);
                        //Debug.DrawRay(Target.transform.InverseTransformPoint(point), _hits[0].normal * 0.1f, Color.blue, 10000);

                        //var curDist = Vector3.Distance(_sphereMesh.vertices[i], Target.transform.InverseTransformPoint(point));
                        var curDist = Vector3.Distance(_sphereMesh.vertices[i], rotation * Vector3.Scale(point, scale));
                        if (curDist < minDist)
                        {
                            minVertIndex = i;
                            minDist = curDist;
                        }
                    }

                    if (_activeVertices.Add(minVertIndex))
                    {
                        //_vertices[_activeVertices.Count - 1] = _sphereMesh.vertices[minVertIndex];
                        //_spawnMesh.SetVertices(_vertices, 0, _activeVertices.Count);

                        foreach (var x in _triangles[_sphereMesh.vertices[minVertIndex]])
                        {
                            _vertices[x] = _sphereMesh.vertices[minVertIndex];
                            //_spawnMesh.vertices[x] = _sphereMesh.vertices[minVertIndex];
                        }
                        //_vertices[minVertIndex] = _sphereMesh.vertices[minVertIndex];

                        _spawnMesh.vertices = _vertices;
                        _spawnMesh.triangles = _sphereMesh.triangles;
                        //_spawnMesh.MarkModified();
                        //_spawnMesh.SetVertices(_vertices);
                    }
                }
            }

            point.w = 1;

            Shader.SetGlobalVector("_Point", point);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public override bool IsNeedDisablingRotation()
        {
            return false;
        }

        public override bool IsAlwaysActive()
        {
            return true;
        }

        protected override void AfterEnable()
        {
            _albedo.SetActiveTexture(_camera);
        }

        protected override void AfterDisable()
        {
            _albedo.SetInactiveTexture(_camera);

            Shader.SetGlobalVector("_Point", Vector4.one * 999);
        }

        protected override void AfterShow()
        {
            FilledColor.EnableParticles();
        }

        protected override IEnumerator AfterInit()
        {
            _vertices = new Vector3[_sphereMesh.vertexCount];

            //var nonDuplicates = Enumerable.Range(0, _sphereMesh.triangles.Length).GroupBy(x => _sphereMesh.vertices[_sphereMesh.triangles[x]], x => x);
            //vert, vertInd
            _triangles = Enumerable.Range(0, _sphereMesh.triangles.Length).ToLookup(x => _sphereMesh.vertices[_sphereMesh.triangles[x]], x => _sphereMesh.triangles[x]);

            _spawnMesh = new Mesh();
            _spawnMesh.MarkDynamic();
            _spawnMesh.vertices = _vertices;
            _spawnMesh.triangles = _sphereMesh.triangles;
            //_spawnMesh.SetVertices(_vertices);

            var shape = PolishParticles.shape;
            shape.mesh = _spawnMesh;
            shape.meshShapeType = ParticleSystemMeshShapeType.Triangle;
            shape.shapeType = ParticleSystemShapeType.Mesh;
            shape.position = Target.mesh.bounds.center;
            shape.scale = GetScaleFromTo(_sphereMesh.bounds.size, Vector3.Scale(Target.mesh.bounds.size, Target.transform.localScale));

            _camera = Camera.main;
            _albedo = new PaintableTexture(Color.white - Color.black, BaseTexture.width, BaseTexture.height, Constants.Specular, _uvShader, Target.mesh, _fixIlsandEdgesShader, MarkedIslands);

            var hasntTexture = PaintablesHolder.TryAddTexture(Constants.Specular, _albedo);
            if (!hasntTexture)
            {
                _albedo = PaintablesHolder.GetTexture(Constants.Specular);
            }

            //_filledColor.SetBaseTexture(_startTex);
            //_filledColor.SetTexture(_albedo.RuntimeTexture);
            //_filledColor.SetTargetColor(Color.white);
            _meshMaterial.SetTexture(_albedo.Id, _albedo.RuntimeTexture);

            _albedo.SetActiveTexture(_camera);
            if (hasntTexture)
            {
                _albedo.BlitWithTexture(BaseTexture, _setupShader, Color.white - Color.black, null);
            }

            _inited = true;

            yield return null;
            _startTex = new RenderTexture(_albedo.RuntimeTexture.descriptor);
            _startTex.name = $"StartTex {GetType().Name}";
            FillWithColor(_startTex, _fillShader);
            yield return SetupDestinationTextureBuffer();
            FillWithColor(_albedo.PaintedTexture, _fillColorShader);

            Shader.SetGlobalVector("_Point", Vector4.one * 999);
        }

        public void FillWithColor(RenderTexture source, Shader shader)
        {
            var mat = new Material(shader);
            mat.color = Color.white - Color.white;

            Graphics.Blit(source, source, mat);
        }

        private Vector3 GetScaleFromTo(Vector3 from, Vector3 to)
        {
            var x = 1 / (from.x / to.x);
            var y = 1 / (from.y / to.y);
            var z = 1 / (from.z / to.z);

            return new Vector3(x, y, z);
        }

        private IEnumerator SetupDestinationTextureBuffer()
        {
            _startTexBuffer = new CommandBuffer();
            _startTexBuffer.name = "startTex";
            _startTexBuffer.SetRenderTarget(_startTex);

            var fill = new Material(_fillColorShader);
            if (UVMask != null)
            {
                fill.SetTexture("_UVMask", UVMask);
            }
            if (!fill.SetPass(0))
            {
                Debug.LogError("Invalid Shader Pass: ");
            }

            fill.color = Color.white;
            _startTexBuffer.DrawMesh(Target.mesh, Matrix4x4.identity, fill);

            _camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, _startTexBuffer);

            for (int i = 0; i < 5; i++)
            {
                yield return null;
            }

            _camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, _startTexBuffer);
        }
    }
}
