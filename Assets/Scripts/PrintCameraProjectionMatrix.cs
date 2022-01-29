using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Astetrio.Spaceship
{
    public class PrintCameraProjectionMatrix : MonoBehaviour
    {
        private void Start()
        {
            var m1 = GetComponent<Camera>().projectionMatrix;
            var m2 = GetComponent<Camera>().worldToCameraMatrix;

            Debug.Log(m1);
            Debug.Log(m2);
        }
    }
}
