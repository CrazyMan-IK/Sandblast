using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sandblast.UI
{
    public class InstrumentView : MonoBehaviour, IPointerClickHandler
    {
        public event Action<InstrumentView> Clicked = null;

        [SerializeField] private Instrument _instrument = null;
        [SerializeField] private RectTransform _view = null;

        public Instrument Instrument => _instrument;

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        public void Enable()
        {
            _instrument.Enable();
            _view.DOLocalMoveY(200, 0.2f);
        }

        public void Disable()
        {
            _instrument.Disable();
            _view.DOLocalMoveY(0, 0.2f);
        }
    }
}
