using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sandblast
{
    public class Brush : Instrument
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

        private Camera _camera;

        private RenderTexture _markedIlsandes;
        private CommandBuffer _markingIlsdandsBuffer;
        private int _numberOfFrames;
        private RenderTexture _startTex;
        private CommandBuffer _startTexBuffer;

        private PaintableTexture _albedo;
        
        public RenderTexture RuntimeTexture { get; set; }
        public RenderTexture PaintedTexture { get; set; }

        private void Awake()
        {
            _camera = Camera.main;
        }

        private void Start()
        {
            _markedIlsandes = new RenderTexture(_baseTexture.width, _baseTexture.height, 0, RenderTextureFormat.R8);
            _albedo = new PaintableTexture(Color.white, _baseTexture.width, _baseTexture.height, "_BaseMap", _uvShader, _target.mesh, _fixIlsandEdgesShader, _markedIlsandes, RuntimeTexture, PaintedTexture);

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
            fill.color = _targetColor;

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
            _filledColor.SetTargetColor(_targetColor);

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            var point = Vector4.positiveInfinity;

            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider.gameObject == _target.gameObject)
                {
                    point = hit.point;
                }
            }

            point.w = Input.GetMouseButton(0) ? 1 : 0;

            Shader.SetGlobalVector("_Point", point);
            Shader.SetGlobalColor("_BrushColor", _targetColor);
            Shader.SetGlobalTexture("_ErosionTexture", _erosionTexture);
            Shader.SetGlobalFloat("_BrushSize", 0.4f);
            Shader.SetGlobalFloat("_BrushHardness", 0.75f);
        }

        public override bool IsNeedDisablingRotation()
        {
            return true;
        }

        public override void Enable()
        {
            enabled = true;
            _albedo.SetActiveTexture(_camera);
        }

        public override void Disable()
        {
            enabled = false;
            _albedo.SetInactiveTexture(_camera);
        }
    }
}
