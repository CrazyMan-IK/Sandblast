using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sandblast.Interfaces;
using Sandblast.Extensions;

namespace Sandblast.Tutorials
{
    public class PolishTutorial : ITutorial
    {
        private readonly Instrument _instrument = null;
        private readonly RectTransform _preview = null;
        private readonly RectTransform _arrows = null;

        private int _step = 0;

        public PolishTutorial(Instrument instrument, RectTransform preview, RectTransform arrows)
        {
            _instrument = instrument != null ? instrument : throw new ArgumentNullException(nameof(_instrument));
            _preview = preview != null ? preview : throw new ArgumentNullException(nameof(preview));
            _arrows = arrows != null ? arrows : throw new ArgumentNullException(nameof(arrows));
        }

        public IEnumerator StartTutorial()
        {
            _step = 0;

            _preview.gameObject.SetActive(false);
            _arrows.gameObject.SetActive(true);

            while (true)
            {
                switch (_step)
                {
                    case 0:
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
    }
}
