using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    public abstract class Instrument : MonoBehaviour
    {
        public event Action Completed = null;

        [SerializeField] private MarkingIslandsTexture _markingIslands = null;

        private bool _isCompleted = false;

        public bool IsCompleted => _isCompleted;
        protected Texture MarkedIslands => _markingIslands.Result;

        protected void InvokeCompletedEvent()
        {
            _isCompleted = true;
            Completed?.Invoke();
        }

        public abstract bool IsNeedDisablingRotation();
        public abstract void Enable();
        public abstract void Disable();
    }
}
