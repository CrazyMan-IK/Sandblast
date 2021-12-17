using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    public class OrbitalMovement : MonoBehaviour
    {
        [SerializeField] private InputPanel _input = null;
        //[SerializeField] private Transform _verticalAxis = null;
        [SerializeField] private Vector2 _sensitivityScale = Vector2.one * 60;

        private void Update()
        {
            var velocity = _input.Delta * _sensitivityScale * Time.deltaTime;

            //transform.Rotate(0, -velocity.x, 0);
            //_verticalAxis.Rotate(velocity.y, 0, 0, Space.World);
            transform.rotation = Quaternion.AngleAxis(velocity.x, Vector3.down) * Quaternion.AngleAxis(velocity.y, Vector3.right) * transform.rotation;
            //transform.rotation = Quaternion.AngleAxis(1, Vector3.down) * Quaternion.AngleAxis(1, Vector3.right) * transform.rotation;

            //transform.Rotate(-velocity.y, 0, 0);
            //_verticalAxis.Rotate(0, velocity.x, 0);
            //transform.rotation = Quaternion.

            //transform.rotation *= Quaternion.Euler(velocity.y, -velocity.x, 0);
            //transform.rotation *= Quaternion.AngleAxis(velocity.x, Vector3.down);
            //transform.rotation *= Quaternion.AngleAxis(velocity.y, Vector3.right);
            //var x = Mathf.Clamp(transform.eulerAngles.x - velocity.y, -180, 90);
            //var y = transform.eulerAngles.y + velocity.x;
            //var z = transform.eulerAngles.z;

            //transform.eulerAngles = new Vector3(x, y, z);
        }
    }
}
