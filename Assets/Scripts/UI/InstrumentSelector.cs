using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast.UI
{
    [DefaultExecutionOrder(1)]
    public class InstrumentSelector : MonoBehaviour
    {
        [SerializeField] private List<InstrumentView> _instruments = null;
        [SerializeField] private FilledColor _filled = null;
        [SerializeField] private OrbitalMovement _movement = null;

        private int _currentIndex = -1;

        private void Start()
        {
            Select(0);
        }

        private void OnEnable()
        {
            foreach (var instrument in _instruments)
            {
                instrument.Clicked += OnInstrumentClicked;
            }
        }

        private void OnDisable()
        {
            foreach (var instrument in _instruments)
            {
                instrument.Clicked -= OnInstrumentClicked;
            }
        }

        public void Select(int index)
        {
            if (index < 0 || index >= _instruments.Count || (_currentIndex >= 0 && _currentIndex > index))
            {
                return;
            }

            var view = _instruments[index];
            var isEnabled = view.Instrument.enabled && _currentIndex >= 0;

            foreach (var instrument in _instruments)
            {
                instrument.Disable();
            }

            if (!isEnabled)
            {
                view.Enable();
            } 
            if (view.Instrument.IsNeedDisablingRotation() && !isEnabled)
            {
                _movement.enabled = false;
            }
            else
            {
                _movement.enabled = true;
            }
            _currentIndex = index;
        }

        public void Select(InstrumentView view)
        {
            var newIndex = _instruments.FindIndex(x => x == view);
            Select(newIndex);
        }

        private void OnInstrumentClicked(InstrumentView sender)
        {
            if (_filled.IsFilled() || sender == _instruments[_currentIndex])
            {
                Select(sender);
            }
        }
    }
}
