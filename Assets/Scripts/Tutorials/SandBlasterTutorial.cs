using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sandblast.Interfaces;
using Sandblast.Extensions;

namespace Sandblast.Tutorials
{
    public class SandBlasterTutorial : ITutorial
    {
        private readonly Instrument _instrument = null;
        private readonly Button _toggleButton = null;
        private readonly RectTransform _preview = null;
        private readonly RectTransform _arrows = null;

        private int _step = 0;

        public SandBlasterTutorial(Instrument instrument, Button toggleButton, RectTransform preview, RectTransform arrows)
        {
            _instrument = instrument != null ? instrument : throw new ArgumentNullException(nameof(_instrument));
            _toggleButton = toggleButton != null ? toggleButton : throw new ArgumentNullException(nameof(toggleButton));
            _preview = preview != null ? preview : throw new ArgumentNullException(nameof(preview));
            _arrows = arrows != null ? arrows : throw new ArgumentNullException(nameof(arrows));
        }

        public IEnumerator StartTutorial()
        {
            _step = 0;

            _preview.gameObject.SetActive(true);
            _arrows.gameObject.SetActive(false);

            _toggleButton.onClick.AddListener(OnButtonClicked);

            while (true)
            {
                switch (_step)
                {
                    case 0:
                    {
                        var btnTransform = _toggleButton.transform as RectTransform;
                        _preview.position = btnTransform.position;
                        _preview.sizeDelta = btnTransform.rect.size * Mathf.Sin(Time.time * 5).Remap(-1, 1, 0.8f, 1);
                        break;
                    }
                    case 1:
                    {
                        _arrows.localScale = Vector2.one * Mathf.Sin(Time.time * 5).Remap(-1, 1, 0.8f, 1);
                        if (Input.GetMouseButtonDown(0))
                        {
                            _step++;
                            _arrows.gameObject.SetActive(false);
                            PlayerPrefs.SetInt($"{_instrument.GetType().Name}-tutorial", 1);
                            yield break;
                        }
                        break;
                    }
                }

                yield return null;
            }
        }

        private void OnButtonClicked()
        {
            _step++;
            _preview.gameObject.SetActive(false);
            _arrows.gameObject.SetActive(true);

            _toggleButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
