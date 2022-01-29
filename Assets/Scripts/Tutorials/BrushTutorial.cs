using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sandblast.Interfaces;
using Sandblast.Extensions;

namespace Sandblast.Tutorials
{
    public class BrushTutorial : ITutorial
    {
        private readonly Brush _brush = null;
        private readonly RectTransform _preview = null;
        private readonly RectTransform _arrows = null;
        private readonly RectTransform _brushArrow = null;

        private int _step = 0;

        public BrushTutorial(Brush brush, RectTransform preview, RectTransform arrows, RectTransform brushArrow)
        {
            _brush = brush != null ? brush : throw new ArgumentNullException(nameof(brush));
            _preview = preview != null ? preview : throw new ArgumentNullException(nameof(preview));
            _arrows = arrows != null ? arrows : throw new ArgumentNullException(nameof(arrows));
            _brushArrow = brushArrow != null ? brushArrow : throw new ArgumentNullException(nameof(brushArrow));
        }

        public IEnumerator StartTutorial()
        {
            _step = 0;

            _preview.gameObject.SetActive(false);
            _arrows.gameObject.SetActive(true);
            _brushArrow.gameObject.SetActive(false);

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
                            _brush.StageChanged += OnBrushStageChanged;
                        }
                        break;
                    }
                    case 1:
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            _step++;
                            //_preview.gameObject.SetActive(true);
                            _brushArrow.gameObject.SetActive(true);
                        }
                        break;
                    }
                    case 2:
                    {
                        var rect = ToScreenSpace(_brush.LastJar.Bounds);
                        var paintPos = WorldToGUIPoint(_brush.PaintPoint.position);
                        _brushArrow.position = (paintPos + rect.center) / 2;

                        var newSize = _brushArrow.sizeDelta;
                        newSize.x = Vector3.Distance(rect.center, paintPos);
                        _brushArrow.sizeDelta = newSize;

                        var newRotation = _brushArrow.eulerAngles;
                        newRotation.z = Vector2.SignedAngle(Vector2.right, rect.center - paintPos);
                        _brushArrow.eulerAngles = newRotation;

                        break;
                    }
                    case 3:
                    {
                        PlayerPrefs.SetInt($"{_brush.GetType().Name}-tutorial", 1);
                        yield break;
                    }
                }

                yield return null;
            }
        }

        private Vector2 WorldToGUIPoint(Vector3 world)
        {
            return Camera.main.WorldToScreenPoint(world);
        }

        private Rect ToScreenSpace(Bounds bounds)
        {
            var cen = bounds.center;
            var ext = bounds.extents;

            Vector2[] extentPoints = new Vector2[8]
            {
                WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
                WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
                WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
                WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
                WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
                WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
                WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
                WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
            };

            var min = extentPoints[0];
            var max = extentPoints[0];
            foreach (var v in extentPoints)
            {
                min = Vector2.Min(min, v);
                max = Vector2.Max(max, v);
            }

            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        private void OnBrushStageChanged()
        {
            if (_step == 0)
            {
                _step += 3;
            }
            else
            {
                _step++;
                _brushArrow.gameObject.SetActive(false);
            }

            _brush.StageChanged -= OnBrushStageChanged;
        }
    }
}
