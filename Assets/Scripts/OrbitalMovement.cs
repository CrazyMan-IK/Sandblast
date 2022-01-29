using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    public class OrbitalMovement : MonoBehaviour
    {
        [SerializeField] private InputPanel _input = null;
        [SerializeField] private Vector2 _sensitivityScale = Vector2.one * 60;

        private void Update()
        {
            var velocity = _input.Delta * _sensitivityScale;// * Time.deltaTime;

            transform.rotation = Quaternion.AngleAxis(velocity.x, Vector3.down) * Quaternion.AngleAxis(velocity.y, Vector3.right) * transform.rotation;
        }
    }
}
