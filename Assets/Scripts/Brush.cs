using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;
using Sandblast.Models;
using DitzelGames.FastIK;

namespace Sandblast
{
    public class Brush : Instrument, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        public event Action StageChanged = null;

        [SerializeField] private FastIKFabric _ik = null;
        [SerializeField] private PaintingJar _jarPrefab = null;
        [SerializeField] private Material _brushMaterial = null;
        [SerializeField] private Material _meshMaterial = null;
        [SerializeField] private Shader _uvShader = null;
        [SerializeField] private Shader _setupShader = null;
        [SerializeField] private Shader _fillShader = null;
        [SerializeField] private Shader _fillColorShader = null;
        [SerializeField] private Shader _fixIlsandEdgesShader = null;
        [SerializeField] private Transform _paintPoint = null;

        public readonly List<BrushStage> _stages = new List<BrushStage>();

        private Camera _camera;

        private RenderTexture _startTex;
        private CommandBuffer _startTexBuffer;

        private PaintableTexture _albedo;
        private bool _inited = false;
        private Vector4 _point = Vector4.one * 999;

        private Transform _targetPoint = null;
        private readonly Collider[] _lastColliders = new Collider[4];

        public Transform PaintPoint => _paintPoint;
        public PaintingJar LastJar { get; private set; } = null;

        private void Awake()
        {
            _targetPoint = new GameObject("Brush target").transform;
            _targetPoint.parent = transform.parent;

            _ik.Target = _targetPoint;
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
            FilledColor.SetUVMask(_startTex);
            FilledColor.SetTargetColorUsage(false);

            _targetPoint.position = Vector3.Lerp(_targetPoint.position, _paintPoint.position, 20 * Time.deltaTime);

            Shader.SetGlobalVector("_Point", _point);
            Shader.SetGlobalColor("_BrushColor", TargetColor);
            Shader.SetGlobalFloat("_BrushSize", 0.250f);
            Shader.SetGlobalFloat("_BrushHardness", 0.75f);

            if (Input.GetMouseButtonUp(0) && FilledColor.IsFilled() && !IsCompleted)
            {
                StartCoroutine(_albedo.BlitWithTexture(_startTex, _setupShader, Color.white, null, true));
                InvokeCompletedEvent();
            }
        }

        /*private void OnTriggerEnter(Collider collider)
        {
            if (collider.TryGetComponent<PaintingJar>(out var jar))
            {
                Reinit(jar.Stage.Color, jar.Stage.UVMask);
            }
        }*/

        public void OnBeginDrag(PointerEventData eventData)
        {
            _point.w = 1;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _point = Vector4.one * 999;

            var coord = _camera.ScreenToWorldPoint((Vector3)eventData.position + Vector3.forward * 1.45f);
            transform.position = Vector3.Slerp(transform.position, coord, 20 * Time.deltaTime);

            Ray ray = new Ray(_paintPoint.position, _paintPoint.position - _camera.transform.position);

            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider.gameObject == Target.gameObject)
                {
                    _point = hit.point;
                }
            }

            var count = Physics.OverlapSphereNonAlloc(_paintPoint.position, 0.05f, _lastColliders);
            if (count > 0 && _lastColliders[0].TryGetComponent(out PaintingJar jar) && TargetColor != jar.Stage.Color)
            {
                Reinit(jar.Stage.Color, jar.Stage.UVMask);

                _brushMaterial.SetColor("_Color", TargetColor);
                Color.RGBToHSV(TargetColor, out var H, out var S, out var V);
                V -= 0.3f;
                _brushMaterial.SetColor("_ColorDim", Color.HSVToRGB(H, S, V));

                StageChanged?.Invoke();
            }

            _point.w = 1;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _point.w = 0;
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public void AddStage(Color color, Texture uvMask)
        {
            _stages.Add(new BrushStage() { Color = color, UVMask = uvMask });
        }

        public void Init()
        {
            for (int i = 0; i < _stages.Count; i++)
            {
                var stage = _stages[i];

                var jar = Instantiate(_jarPrefab, transform.position + Vector3.down * 0.75f + ((_stages.Count / 2 - i) * 0.25f - 0.125f) * Vector3.left, _jarPrefab.transform.rotation, transform.parent);
                jar.Init(stage);

                if (i == _stages.Count - 1)
                {
                    LastJar = jar;
                }
            }

            StartCoroutine(AfterCustomInit());
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
            _brushMaterial.color = TargetColor;

            _albedo.SetActiveTexture(_camera);
        }

        protected override void AfterDisable()
        {
            _albedo.SetInactiveTexture(_camera);

            Shader.SetGlobalVector("_Point", Vector4.one * 999);
        }

        protected override void AfterShow()
        {
            _brushMaterial.SetColor("_Color", TargetColor);
            Color.RGBToHSV(TargetColor, out var H, out var S, out var V);
            V -= 0.3f;
            _brushMaterial.SetColor("_ColorDim", Color.HSVToRGB(H, S, V));
        }

        public void FillWithColor(RenderTexture source, Shader shader)
        {
            var temp = RenderTexture.GetTemporary(source.descriptor);

            var mat = new Material(shader);
            mat.color = Color.white - Color.white;

            Graphics.Blit(temp, source, mat);
            Graphics.Blit(source, temp, mat);

            RenderTexture.ReleaseTemporary(temp);
        }

        private IEnumerator AfterCustomInit()
        {
            _camera = Camera.main;
            _albedo = new PaintableTexture(Color.white, BaseTexture.width, BaseTexture.height, Constants.Albedo, _uvShader, Target.mesh, _fixIlsandEdgesShader, MarkedIslands);

            var hasntTexture = PaintablesHolder.TryAddTexture(Constants.Albedo, _albedo);
            if (!hasntTexture)
            {
                _albedo = PaintablesHolder.GetTexture(Constants.Albedo);
            }

            //_filledColor.SetBaseTexture(_startTex);
            //_filledColor.SetTexture(_albedo.RuntimeTexture);
            //_filledColor.SetTargetColor(Color.white);
            _meshMaterial.SetTexture(_albedo.Id, _albedo.RuntimeTexture);

            _albedo.SetActiveTexture(_camera);
            if (hasntTexture)
            {
                _albedo.BlitWithTexture(BaseTexture, _setupShader, Color.white, null);
            }

            _inited = true;

            yield return null;
            _startTex = new RenderTexture(_albedo.RuntimeTexture.descriptor);
            _startTex.name = $"StartTex {GetType().Name}";
            FillWithColor(_startTex, _fillShader);

            var temp = RenderTexture.GetTemporary(_startTex.descriptor);

            yield return SetupDestinationTextureBuffer(temp);

            RenderTexture.ReleaseTemporary(temp);

            Shader.SetGlobalVector("_Point", Vector4.one * 999);
        }

        private IEnumerator SetupDestinationTextureBuffer(RenderTexture target)
        {
            _startTexBuffer = new CommandBuffer();
            _startTexBuffer.name = "startTex";
            _startTexBuffer.SetRenderTarget(target);

            for (int i = 0; i < _stages.Count; i++)
            {
                var stage = _stages[i];

                /*var copy = new Material(_setupShader);
                copy.SetTexture("_SecondTex", _startTex);
                copy.color = Color.white;*/

                var fill = new Material(_fillColorShader);
                if (stage.UVMask != null)
                {
                    //copy.SetTexture("_UVMask", stage.UVMask);
                    fill.SetTexture("_UVMask", stage.UVMask);
                }

                fill.color = stage.Color;
                fill.mainTexture = _startTex;
                _startTexBuffer.DrawMesh(Target.mesh, Matrix4x4.identity, fill);
                //_startTexBuffer.Blit(target, _startTex, copy);
                _startTexBuffer.CopyTexture(target, _startTex);
                //_startTexBuffer.Blit(_startTex, target, copy);
                //_startTexBuffer.SetRenderTarget(target);
            }

            //UnityEditorInternal.RenderDoc.BeginCaptureRenderDoc(UnityEditor.EditorWindow.focusedWindow);
            _camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, _startTexBuffer);

            for (int i = 0; i < 5; i++)
            {
                yield return null;
            }

            _camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, _startTexBuffer);
            //UnityEditorInternal.RenderDoc.EndCaptureRenderDoc(UnityEditor.EditorWindow.focusedWindow);

            //Graphics.Blit(target, _startTex);
        }
    }
}
