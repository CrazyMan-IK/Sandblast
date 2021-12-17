using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sandblast
{
    [DefaultExecutionOrder(-1)]
    [RequireComponent(typeof(ParticleSystem))]
    public class SandBlaster : Instrument
    {
        [SerializeField] private Texture _baseTexture;
        [SerializeField] private Texture _erosionTexture;
        [SerializeField] private Material _meshMaterial;
        [SerializeField] private MeshFilter _target;
        [SerializeField] private Shader _uvShader;
        [SerializeField] private Shader _setupShader;
        [SerializeField] private Shader _fillShader;
        [SerializeField] private Shader _ilsandMarkerShader;
        [SerializeField] private Shader _fixIlsandEdgesShader;
        [SerializeField] private FilledColor _filledColor;
        [SerializeField] private Color _targetColor = Color.white;
        [SerializeField] private Brush _brush = null;

        private Camera _camera;
        private ParticleSystem _particleSystem;
        private readonly List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();
        private readonly Vector4[] _points = new Vector4[128];

        private RenderTexture _markedIlsandes;
        private CommandBuffer _markingIlsdandsBuffer;
        private int _numberOfFrames;
        private RenderTexture _startTex;
        private CommandBuffer _startTexBuffer;

        private PaintableTexture _albedo;

        private void Awake()
        {
            _camera = Camera.main;
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Start()
        {
            _markedIlsandes = new RenderTexture(_baseTexture.width, _baseTexture.height, 0, RenderTextureFormat.R8);
            _albedo = new PaintableTexture(Color.white, _baseTexture.width, _baseTexture.height, "_BaseMap", _uvShader, _target.mesh, _fixIlsandEdgesShader, _markedIlsandes);
            _brush.RuntimeTexture = _albedo.RuntimeTexture;
            _brush.PaintedTexture = _albedo.PaintedTexture;

            _startTex = new RenderTexture(_albedo.RuntimeTexture.descriptor);

            _filledColor.SetBaseTexture(_startTex);
            _filledColor.SetTexture(_albedo.RuntimeTexture);
            _filledColor.SetTargetColor(_targetColor);
            _meshMaterial.SetTexture(_albedo.Id, _albedo.RuntimeTexture);

            _markingIlsdandsBuffer = new CommandBuffer();
            _markingIlsdandsBuffer.name = "markingIlsnads";
            _markingIlsdandsBuffer.SetRenderTarget(_markedIlsandes);
            Material mIlsandMarker = new Material(_ilsandMarkerShader);
            _markingIlsdandsBuffer.DrawMesh(_target.mesh, Matrix4x4.identity, mIlsandMarker);
            _camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, _markingIlsdandsBuffer);

            _albedo.SetActiveTexture(_camera);
            _albedo.BlitWithTexture(_erosionTexture, _setupShader);
            DrawStartUV();

            Shader.SetGlobalInt("_PointsCount", 0);
        }

        private void DrawStartUV()
        {
            _startTexBuffer = new CommandBuffer();
            _startTexBuffer.name = "startTex";
            _startTexBuffer.SetRenderTarget(_startTex);

            var fill = new Material(_fillShader);
            if (!fill.SetPass(0))
            {
                Debug.LogError("Invalid Shader Pass: ");
            }
            fill.color = new Color(1, 1, 0.98f, 1);

            _startTexBuffer.DrawMesh(_target.mesh, Matrix4x4.identity, fill);

            _camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, _startTexBuffer);
        }

        private void Update()
        {
            if (_numberOfFrames > 1)
            {
                _camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, _markingIlsdandsBuffer);
            }

            _numberOfFrames++;

            _albedo.UpdateShaderParameters(_target.transform.localToWorldMatrix);

            _filledColor.SetBaseTexture(_startTex);
            _filledColor.SetTexture(_albedo.RuntimeTexture);
            Shader.SetGlobalColor("_BrushColor", Color.green);
            Shader.SetGlobalTexture("_ErosionTexture", _erosionTexture);
            Shader.SetGlobalFloat("_BrushSize", 0.125f);
            Shader.SetGlobalFloat("_BrushHardness", 0.75f);
        }

        private void OnParticleCollision(GameObject other)
        {
            if (_particleSystem == null)
            {
                return;
            }

            _particleSystem.GetCollisionEvents(other, _collisionEvents);

            for (int i = 0; i < _collisionEvents.Count && i < _points.Length; i++)
            {
                var contactPoint = _collisionEvents[i].intersection;
                _points[i] = new Vector4(contactPoint.x, contactPoint.y, contactPoint.z, 1);
            }

            _filledColor.SetBaseTexture(_startTex);
            _filledColor.SetTexture(_albedo.RuntimeTexture);
            _filledColor.SetTargetColor(_targetColor);
            Shader.SetGlobalVectorArray("_CPoints", _points);
            Shader.SetGlobalInt("_PointsCount", _collisionEvents.Count);
        }

        public override void Enable()
        {
            enabled = true;
            _albedo.SetActiveTexture(_camera);

            var emission = _particleSystem.emission;
            emission.enabled = true;
            //_particleSystem.emission.enabled = true;
        }

        public override void Disable()
        {
            enabled = false;
            _albedo.SetInactiveTexture(_camera);

            var emission = _particleSystem.emission;
            emission.enabled = false;
            //_particleSystem.emission.enabled = false;

            Shader.SetGlobalInt("_PointsCount", 0);
        }
    }
}
