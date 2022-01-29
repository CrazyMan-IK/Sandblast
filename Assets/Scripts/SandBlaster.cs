using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;

namespace Sandblast
{
    [RequireComponent(typeof(ParticleSystem))]
    [RequireComponent(typeof(LookAtConstraint))]
    public class SandBlaster : Instrument
    {
        [SerializeField] private Material _meshMaterial;
        [SerializeField] private Shader _uvShader;
        [SerializeField] private Shader _setupShader;
        [SerializeField] private Shader _fillShader;
        [SerializeField] private Shader _fillColorShader;
        [SerializeField] private Shader _fixIlsandEdgesShader;

        private Camera _camera;
        private ParticleSystem _particleSystem;
        private LookAtConstraint _lookAt;

        private RenderTexture _startTex;
        private CommandBuffer _startTexBuffer;

        private PaintableTexture _albedo;
        private bool _inited = false;

        private Ray _ray = new Ray();
        private readonly RaycastHit[] _hits = new RaycastHit[1];

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _lookAt = GetComponent<LookAtConstraint>();
        }

        private void Update()
        {
            if (!_inited)
            {
                return;
            }

            _albedo.UpdateShaderParameters(Target.transform.localToWorldMatrix, UVMask);

            FilledColor.SetBaseTexture(_startTex);
            FilledColor.SetTexture(_albedo.RuntimeTexture);
            FilledColor.SetTargetColor(TargetColor);
            FilledColor.SetUVMask(UVMask);
            if (UVMask != null)
            {
                FilledColor.SetUVMask(UVMask);
            }
            else
            {
                FilledColor.SetUVMask(_startTex);
            }

            const float rotationThreshold = 1.25f;

            _ray.origin = transform.position;
            _ray.direction = Quaternion.Euler(Random.Range(-rotationThreshold, rotationThreshold), Random.Range(-rotationThreshold, rotationThreshold), 0) * transform.forward;
            var point = Vector4.one * 999;

            if (Physics.RaycastNonAlloc(_ray, _hits) > 0)
            {
                if (_hits[0].collider.gameObject == Target.gameObject)
                {
                    point = _hits[0].point;
                }
            }

            point.w = 1;

            Shader.SetGlobalVector("_Point", point);
            Shader.SetGlobalColor("_BrushColor", TargetColor);
            Shader.SetGlobalFloat("_BrushSize", 0.250f);
            Shader.SetGlobalFloat("_BrushHardness", 0.75f);

            if (Input.GetMouseButtonUp(0) && FilledColor.IsFilled() && !IsCompleted)
            {
                StartCoroutine(_albedo.BlitWithTexture(_startTex, _setupShader, Color.white, UVMask, true));
                InvokeCompletedEvent();
            }
        }

        public override bool IsNeedDisablingRotation()
        {
            return false;
        }

        public override bool IsAlwaysActive()
        {
            return false;
        }

        protected override void AfterEnable()
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }

            //_albedo.ReplacePaintShader(_uvShader);
            _albedo.SetActiveTexture(_camera);

            var emission = _particleSystem.emission;
            emission.enabled = true;
        }

        protected override void AfterDisable()
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }

            _albedo.SetInactiveTexture(_camera);

            var emission = _particleSystem.emission;
            emission.enabled = false;

            Shader.SetGlobalVector("_Point", Vector4.one * 999);
        }

        protected override IEnumerator AfterInit()
        {
            _lookAt.SetSource(0, new ConstraintSource() { sourceTransform = Target.transform.parent, weight = 1 });

            _camera = Camera.main;
            _albedo = new PaintableTexture(Color.white, BaseTexture.width, BaseTexture.height, Constants.Albedo, _uvShader, Target.mesh, _fixIlsandEdgesShader, MarkedIslands);

            var hasntTexture = PaintablesHolder.TryAddTexture(Constants.Albedo, _albedo);
            if (!hasntTexture)
            {
                _albedo = PaintablesHolder.GetTexture(Constants.Albedo);
            }

            //_filledColor.SetBaseTexture(_startTex);
            //_filledColor.SetTexture(_albedo.RuntimeTexture);
            //_filledColor.SetTargetColor(_targetColor);
            _meshMaterial.SetTexture(_albedo.Id, _albedo.RuntimeTexture);

            _albedo.SetActiveTexture(_camera);
            if (hasntTexture)
            {
                _albedo.BlitWithTexture(BaseTexture, _setupShader, Color.white, UVMask);
            }

            _inited = true;

            yield return null;
            _startTex = new RenderTexture(_albedo.RuntimeTexture.descriptor);
            _startTex.name = $"StartTex {GetType().Name}";
            FillWithColor(_startTex, _fillShader);
            yield return SetupDestinationTextureBuffer();

            Shader.SetGlobalVector("_Point", Vector4.one * 999);
        }

        public void FillWithColor(RenderTexture source, Shader shader)
        {
            var mat = new Material(shader);
            mat.color = Color.white - Color.white;

            Graphics.Blit(source, source, mat);
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

            fill.color = TargetColor;
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
