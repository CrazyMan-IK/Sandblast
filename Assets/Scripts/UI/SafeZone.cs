using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyGames.Logires
{
    public class SafeZone : MonoBehaviour
    {
        private RectTransform _transform = null;
        private Rect _lastSafeArea = Rect.zero;

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();

            TryUpdateSafeArea();
        }

        private void Update()
        {
#if UNITY_EDITOR
            TryUpdateSafeArea();
#endif
        }

        private void TryUpdateSafeArea()
        {
            var safeArea = Screen.safeArea;

            if (safeArea != _lastSafeArea)
            {
                ApplySafeArea(safeArea);
            }
        }

        private void ApplySafeArea(Rect area)
        {
            var anchorMin = area.position;
            var anchorMax = area.position + area.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            _transform.anchorMin = anchorMin;
            _transform.anchorMax = anchorMax;

            _lastSafeArea = area;
        }
    }
}
