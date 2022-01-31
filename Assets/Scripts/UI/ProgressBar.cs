using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Sandblast.UI
{
    [RequireComponent(typeof(Slider))]
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private float _speedMultiplier = 5;
        [SerializeField] private Image _firstStage = null;
        [SerializeField] private RectTransform _stagesRoot = null;
        [SerializeField] private Image _lastStage = null;
        [SerializeField] private Image _stagePrefab = null;

        private readonly List<Image> _stages = new List<Image>();

        private Slider _slider = null;
        private float _targetValue = 0;
        private int _instrumentsCount = 0;
        private int _currentInstrument = 0;

        private void Awake()
        {
            _slider = GetComponent<Slider>();
        }

        private void Update()
        {
            _slider.value = Mathf.Lerp(_slider.value, _targetValue, _speedMultiplier * Time.deltaTime);
        }

        public void Init(IReadOnlyList<Instrument> instruments)
        {
            if (instruments == null)
            {
                throw new ArgumentNullException(nameof(instruments));
            }
            if (instruments.Count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(instruments));
            }

            _instrumentsCount = instruments.Count;

            _stages.Add(_firstStage);

            for (int i = 0; i < instruments.Count; i++)
            {
                var instrument = instruments[i];

                if (i == 0)
                {
                    _firstStage.sprite = instrument.Preview;
                    continue;
                }

                var stage = Instantiate(_stagePrefab, _stagesRoot);
                stage.sprite = instrument.Preview;

                _stages.Add(stage);
            }

            _stages.Add(_lastStage);
        }

        public void SetCurrentInstrument(int index)
        {
            _currentInstrument = index;
        }

        public void SetValue(float value)
        {
            if (_instrumentsCount == 0)
            {
                _targetValue = 0;
                return;
            }

            _targetValue = Mathf.Clamp01(value) / _instrumentsCount + 1.0f / _instrumentsCount * _currentInstrument;
        }

        public void SetValueInstantly(float value)
        {
            _slider.value = value;
            SetValue(value);
        }

        public Image GetStage(int index)
        {
            return _stages[index];
        }
    }
}
