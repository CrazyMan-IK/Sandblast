using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    public abstract class Instrument : MonoBehaviour
    {
        public event Action Completed = null;

        [SerializeField] private Sprite _preview = null;
        [SerializeField] private Rect _offset = Rect.zero;

        private bool _isCompleted = false;

        private bool _isFirstInstrument = false;
        private bool _isLastInstrument = false;
        private InputPanel _input = null;
        private Texture _baseTexture = null;
        private Texture _uvMask = null;
        private MarkingIslandsTexture _markingIslands = null;
        private MeshFilter _target = null;
        private FilledColor _filledColor = null;
        private PaintablesHolder _paintablesHolder = null;
        private Color _targetColor = Color.white;
        private float _maxFill = 0;

        public Sprite Preview => _preview;
        public Rect Offset => _offset;
        public bool IsCompleted => _isCompleted;

        protected bool IsFirstInstrument => _isFirstInstrument;
        protected InputPanel InputPanel => _input;
        protected Texture BaseTexture => _baseTexture;
        protected Texture UVMask => _uvMask;
        protected Texture MarkedIslands => _markingIslands.Result;
        protected MeshFilter Target => _target;
        protected FilledColor FilledColor => _filledColor;
        protected PaintablesHolder PaintablesHolder => _paintablesHolder;
        protected Color TargetColor => _targetColor;
        protected float MaxFill => _maxFill;

        protected void InvokeCompletedEvent()
        {
            FilledColor.SetManualValue(0);
            FilledColor.EnableManualMode();

            _isCompleted = true;
            Completed?.Invoke();
        }

        public void Init(bool isFirstInstrument, InputPanel input, Texture baseTexture, Texture uvMask, MarkingIslandsTexture markingIslands, MeshFilter target, FilledColor filledColor, PaintablesHolder paintablesHolder, Color targetColor, float maxFill)
        {
            _isFirstInstrument = isFirstInstrument;
            _input = input;
            _baseTexture = baseTexture;
            _uvMask = uvMask;
            _markingIslands = markingIslands;
            _target = target;
            _filledColor = filledColor;
            _paintablesHolder = paintablesHolder;
            _targetColor = targetColor;
            _maxFill = maxFill;

            StartCoroutine(AfterInit());
        }

        public void Enable()
        {
            enabled = true;

            _filledColor.SetMaxFill(_maxFill);
            FilledColor.DisableManualMode();
            FilledColor.SetTargetColorUsage(true);

            AfterEnable();
        }

        public void Disable()
        {
            enabled = false;

            AfterDisable();
        }

        public void Show()
        {
            gameObject.SetActive(true);

            AfterShow();
        }

        public void Hide()
        {
            gameObject.SetActive(false);

            AfterHide();
        }

        protected void Reinit(Color color, Texture uvMask)
        {
            _targetColor = color;
            _uvMask = uvMask;
        }

        public abstract bool IsNeedDisablingRotation();
        public abstract bool IsAlwaysActive();

        protected virtual void AfterEnable() { }
        protected virtual void AfterDisable() { }
        protected virtual void AfterShow() { }
        protected virtual void AfterHide() { }
        protected virtual IEnumerator AfterInit()
        {
            yield break;
        }
    }
}
