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
            if (index >= 0 && index < _instruments.Count)
            {
                Select(_instruments[index]);
            }
        }

        public void Select(InstrumentView view)
        {
            var newIndex = _instruments.FindIndex(x => x == view);
            if (newIndex < 0 || (_currentIndex > 0 && _currentIndex >= newIndex))
            {
                return;
            }

            foreach (var instrument in _instruments)
            {
                instrument.Disable();
            }

            view.Enable();
            _currentIndex = newIndex;
        }

        private void OnInstrumentClicked(InstrumentView sender)
        {
            if (_filled.GetProgress() >= 99)
            {
                Select(sender);
            }
        }
    }
}
