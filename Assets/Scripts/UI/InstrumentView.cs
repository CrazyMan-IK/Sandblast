using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace Sandblast.UI
{
    public class InstrumentView : MonoBehaviour, IPointerClickHandler
    {
        public event Action<InstrumentView> Clicked = null;

        [SerializeField] private Instrument _instrument = null;
        [SerializeField] private RectTransform _viewPivot = null;
        [SerializeField] private Image _view = null;

        private Sequence _sequence = null;

        public Instrument Instrument => _instrument;

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        public void Enable()
        {
            _instrument.Enable();
            _viewPivot.DOLocalMoveY(200, 0.2f);
        }

        public void Disable()
        {
            _instrument.Disable();
            _viewPivot.DOLocalMoveY(0, 0.2f);
        }

        public void StartHighlight()
        {
            if (_sequence != null)
            {
                return;
            }

            _sequence = Highlight();
        }

        public void StopHighlight()
        {
            if (_sequence == null)
            {
                return;
            }

            _sequence.Kill();
            _sequence = null;

            _view.color = Color.white;
        }

        private Sequence Highlight()
        {
            var sequence = DOTween.Sequence().SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            sequence.Append(_view.DOColor(Color.green, 0.5f).SetEase(Ease.Linear));
            sequence.Append(_view.DOColor(Color.white, 0.5f).SetEase(Ease.Linear));

            return sequence;
        }
    }
}
