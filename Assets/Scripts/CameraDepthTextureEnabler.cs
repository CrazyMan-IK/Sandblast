using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    [RequireComponent(typeof(Camera))]
    public class CameraDepthTextureEnabler : MonoBehaviour
    {
        private void Awake()
        {
            var camera = GetComponent<Camera>();
            camera.depthTextureMode = DepthTextureMode.Depth;
        }
    }
}
