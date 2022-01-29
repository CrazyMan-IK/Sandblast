using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Sandblast.Interfaces;

namespace Sandblast
{
    [DefaultExecutionOrder(-2)]
    public class MarkingIslandsTexture : MonoBehaviour
    {
        [SerializeField] private MeshFilter _target = null;
        [SerializeField] private Shader _islandMarker = null;

        private RenderTexture _result = null;
        private CommandBuffer _buffer = null;

        public Texture Result => _result;

        public void Init(Texture texture)
        {
            _result = new RenderTexture(texture.width, texture.height, 0, RenderTextureFormat.R8);

            _buffer = new CommandBuffer();
            _buffer.name = "markingIlsnads";
            _buffer.SetRenderTarget(_result);
            Material mIlsandMarker = new Material(_islandMarker);
            _buffer.DrawMesh(_target.mesh, Matrix4x4.identity, mIlsandMarker);
            Camera.main.AddCommandBuffer(CameraEvent.AfterDepthTexture, _buffer);

            StartCoroutine(AsyncAwake());
        }

        private IEnumerator AsyncAwake()
        {
            yield return new WaitForEndOfFrame();

            Camera.main.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, _buffer);
        }
    }
}
