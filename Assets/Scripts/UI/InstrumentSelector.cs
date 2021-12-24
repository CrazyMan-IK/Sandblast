using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast.UI
{
    [DefaultExecutionOrder(1)]
    public class InstrumentSelector : MonoBehaviour
    {
        public event Action FullCompleted = null;

        [SerializeField] private List<InstrumentView> _instruments = null;
        [SerializeField] private FilledColor _filled = null;
        [SerializeField] private OrbitalMovement _movement = null;

        private int _availableInstrumentsCount = 0;

        private int _currentIndex = -1;
        private int _completedCount = 0;

        private void Start()
        {
            CommonInit();
        }

        private IEnumerator AsyncStart()
        {
            yield return new WaitForEndOfFrame();

            Select(0);
            _instruments[0].StartHighlight();
        }

        private void OnEnable()
        {
            foreach (var instrument in _instruments)
            {
                instrument.Instrument.Completed += OnInstrumentCompleted;
                instrument.Clicked += OnInstrumentClicked;
            }
        }

        private void OnDisable()
        {
            foreach (var instrument in _instruments)
            {
                instrument.Instrument.Completed -= OnInstrumentCompleted;
                instrument.Clicked -= OnInstrumentClicked;
            }
        }

        public void Init(int availableInstrumentsCount)
        {
            if (availableInstrumentsCount < 1 || availableInstrumentsCount > _instruments.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(availableInstrumentsCount));
            }

            for (int i = availableInstrumentsCount; i < _instruments.Count; i++)
            {
                _instruments[i].gameObject.SetActive(false);
            }

            _availableInstrumentsCount = availableInstrumentsCount;

            CommonInit();
        }

        public void Select(int index)
        {
            if (index < 0 || index >= _availableInstrumentsCount || (_currentIndex >= 0 && _currentIndex > index))
            {
                return;
            }

            var view = _instruments[index];
            var isEnabled = view.Instrument.enabled && _currentIndex >= 0;

            for (int i = 0; i < _availableInstrumentsCount; i++)
            {
                _instruments[i].Disable();
            }

            if (!isEnabled)
            {
                view.StopHighlight();
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

        private void CommonInit()
        {
            if (_currentIndex == -1 && _availableInstrumentsCount != 0)
            {
                Select(0);

                StartCoroutine(AsyncStart());
            }
        }

        private void OnInstrumentCompleted()
        {
            _completedCount++;

            if (_completedCount >= _availableInstrumentsCount)
            {
                _instruments[_availableInstrumentsCount - 1].Disable();
                FullCompleted?.Invoke();
                return;
            }

            _instruments[_completedCount].StartHighlight();
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
