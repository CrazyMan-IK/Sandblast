using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sandblast
{
    public class InputPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        public event Action PointerDowned = null;
        public event Action PointerUpped = null;

        public bool IsTouching { get; private set; }
        public Vector2 Delta { get; private set; }

        private void LateUpdate()
        {
            Delta = Vector2.zero;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            IsTouching = true;
            PointerDowned?.Invoke();
        }

        public void OnDrag(PointerEventData eventData)
        {
            Delta = eventData.delta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            IsTouching = false;
            PointerUpped?.Invoke();
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }
    }
}
