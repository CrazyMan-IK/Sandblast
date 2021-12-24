using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sandblast
{
    public class Polish : Instrument
    {
        [SerializeField] private Texture _baseTexture;
        [SerializeField] private Material _meshMaterial;
        [SerializeField] private MeshFilter _target;
        [SerializeField] private Shader _uvShader;
        [SerializeField] private Shader _fillShader;
        [SerializeField] private Shader _fixIlsandEdgesShader;
        [SerializeField] private FilledColor _filledColor;

        private Camera _camera;
        private ParticleSystem _particleSystem;
        private readonly List<ParticleCollisionEvent> _collisionEvents = new List<ParticleCollisionEvent>();
        private readonly Vector4[] _points = new Vector4[128];

        private RenderTexture _startTex;
        private CommandBuffer _startTexBuffer;

        private PaintableTexture _albedo;
        private bool _inited = false;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (!_inited)
            {
                return;
            }

            _albedo.UpdateShaderParameters(_target.transform.localToWorldMatrix);

            _filledColor.SetBaseTexture(_startTex);
            _filledColor.SetTexture(_albedo.RuntimeTexture);
            _filledColor.SetTargetColor(Color.white);
            Shader.SetGlobalColor("_BrushColor", Color.white);
            Shader.SetGlobalFloat("_BrushSize", 0.175f);
            Shader.SetGlobalFloat("_BrushHardness", 0.75f);

            if (Input.GetMouseButtonUp(0) && _filledColor.IsFilled() && !IsCompleted)
            {
                InvokeCompletedEvent();
            }
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
            _filledColor.SetTargetColor(Color.white);
            Shader.SetGlobalVectorArray("_CPoints", _points);
            Shader.SetGlobalInt("_PointsCount", _collisionEvents.Count);
        }

        public void Init(Color targetColor)
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }

            var main = _particleSystem.main;
            main.startColor = targetColor;

            _camera = Camera.main;
            _albedo = new PaintableTexture(new Color(1, 1, 1, 0), _baseTexture.width, _baseTexture.height, "_SpecGlossMap", _uvShader, _target.mesh, _fixIlsandEdgesShader, MarkedIslands);

            _startTex = new RenderTexture(_albedo.RuntimeTexture.descriptor);

            //_filledColor.SetBaseTexture(_startTex);
            //_filledColor.SetTexture(_albedo.RuntimeTexture);
            //_filledColor.SetTargetColor(Color.white);

            _meshMaterial.SetTexture(_albedo.Id, _albedo.RuntimeTexture);

            _albedo.SetActiveTexture(_camera);
            SetupDestinationTextureBuffer();

            _inited = true;
        }

        public override bool IsNeedDisablingRotation()
        {
            return false;
        }

        public override void Enable()
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }

            enabled = true;
            _albedo.SetActiveTexture(_camera);

            var emission = _particleSystem.emission;
            emission.enabled = true;
        }

        public override void Disable()
        {
            if (_particleSystem == null)
            {
                _particleSystem = GetComponent<ParticleSystem>();
            }

            enabled = false;
            _albedo.SetInactiveTexture(_camera);

            var emission = _particleSystem.emission;
            emission.enabled = false;

            Shader.SetGlobalInt("_PointsCount", 0);
        }

        private void SetupDestinationTextureBuffer()
        {
            _startTexBuffer = new CommandBuffer();
            _startTexBuffer.name = "startTex";
            _startTexBuffer.SetRenderTarget(_startTex);

            var fill = new Material(_fillShader);
            if (!fill.SetPass(0))
            {
                Debug.LogError("Invalid Shader Pass: ");
            }

            Graphics.SetRenderTarget(_startTex);
            fill.color = new Color(0, 0, 0, 1);
            Graphics.Blit(_startTex, _startTex, fill);

            fill.color = Color.white;
            _startTexBuffer.DrawMesh(_target.mesh, Matrix4x4.identity, fill);

            _camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, _startTexBuffer);
        }
    }
}
