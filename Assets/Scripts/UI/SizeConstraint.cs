using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    [RequireComponent(typeof(RectTransform))]
    public class SizeConstraint : MonoBehaviour
    {
        [SerializeField] private RectTransform _target = null;

        RectTransform _transform;

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();

            _transform.sizeDelta = _target.sizeDelta;
        }
    }
}
