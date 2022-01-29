using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Sandblast
{
    [RequireComponent(typeof(SphereCollider))]
    public class FoamBubble : MonoBehaviour
    {
        private const string _TargetTag = "Target";

        private SphereCollider _collider = null;
        private bool _destroyed = false;

        private void Awake()
        {
            _collider = GetComponent<SphereCollider>();

            Vector3[] directions = { Vector3.forward, Vector3.up, Vector3.right, Vector3.back, Vector3.down, Vector3.left };

            bool isCollide = true;
            foreach (var dir in directions)
            {
                if (!Physics.Raycast(transform.position, dir, Mathf.Infinity, LayerMask.NameToLayer(_TargetTag)))
                {
                    isCollide = false;
                }
            }

            if (isCollide)
            {
                Destroy(gameObject);
            }
        }

        public bool TryDestroy()
        {
            if (_destroyed)
            {
                return false;
            }

            _destroyed = true;

            const float minDuration = 0.25f;
            const float maxDuration = 0.5f;

            _collider.enabled = false;

            transform.DOScale(0, Random.Range(minDuration, maxDuration)).OnComplete(() =>
            {
                Destroy(gameObject);
            });

            return true;
        }
    }
}
