using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.EventSystems;

namespace Sandblast
{
    //[RequireComponent(typeof(LookAtConstraint))]
    public class Sponge : Instrument, IDragHandler, IInitializePotentialDragHandler
    {
        //private LookAtConstraint _lookAt;

        private Camera _camera;

        private bool _inited = false;
        private bool _firstTimeEnable = false;

        private int _bubblesCount = 0;
        private int _destroyedBubbles = 0;

        private Ray _ray = new Ray();
        private readonly RaycastHit[] _hits = new RaycastHit[10];

        private void Update()
        {
            if (!_inited)
            {
                return;
            }

            if (Input.GetMouseButtonUp(0) && FilledColor.IsFilled() && !IsCompleted)
            {
                InvokeCompletedEvent();
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            const float rotationThreshold = 1.25f;

            var coord = _camera.ScreenToWorldPoint((Vector3)eventData.position + Vector3.forward * 1.45f);
            transform.position = Vector3.Slerp(transform.position, coord, 20 * Time.deltaTime);

            _ray.origin = transform.position;
            _ray.direction = Quaternion.Euler(Random.Range(-rotationThreshold, rotationThreshold) * 2, Random.Range(-rotationThreshold, rotationThreshold), 0) * (coord - _camera.transform.position);

            var hitCount = Physics.RaycastNonAlloc(_ray, _hits);
            if (hitCount > 0)
            {
                var hit = _hits.GetClosestHit(hitCount);

                if (hit.collider.TryGetComponent(out FoamBubble bubble))
                {
                    if (bubble.TryDestroy())
                    {
                        _destroyedBubbles++;

                        FilledColor.SetManualValue(_destroyedBubbles * 1.0f / _bubblesCount);
                    }
                }
            }
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }

        public override bool IsNeedDisablingRotation()
        {
            return false;
        }

        public override bool IsAlwaysActive()
        {
            return true;
        }

        protected override void AfterEnable()
        {
            if (!_firstTimeEnable)
            {
                _bubblesCount = Target.transform.parent.GetComponentsInChildren<FoamBubble>().Length;
            }
            _firstTimeEnable = true;

            FilledColor.EnableManualMode();
        }

        protected override void AfterDisable()
        {
            FilledColor.DisableManualMode();
        }

        protected override IEnumerator AfterInit()
        {
            //_lookAt.SetSource(0, new ConstraintSource() { sourceTransform = Target.transform.parent, weight = 1 });

            _camera = Camera.main;

            _inited = true;

            yield return null;
        }
    }
}
