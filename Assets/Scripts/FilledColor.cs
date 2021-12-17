using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    public class FilledColor : MonoBehaviour
    {
        [SerializeField] private ComputeShader _shader = null;
        [SerializeField] private RenderTexture _baseTexture = null;
        [SerializeField] private RenderTexture _texture = null;
        [SerializeField] private Color _targetColor = Color.white;
        [SerializeField] private TMPro.TextMeshProUGUI _text = null;

        private ComputeBuffer _buffer = null;
        private readonly uint[] _data = new uint[2] { 0, 0 };

        private int _mainKernelHandle = -1;
        private int _pixelsKernelHandle = -1;

        private void Awake()
        {
            _mainKernelHandle = _shader.FindKernel("CSMain");
            _pixelsKernelHandle = _shader.FindKernel("CSPixels");
            _buffer = new ComputeBuffer(2, sizeof(uint));

            _shader.SetBuffer(_mainKernelHandle, "result", _buffer);
            _shader.SetBuffer(_pixelsKernelHandle, "result", _buffer);
        }

        private void OnDestroy()
        {
            _buffer.Dispose();
        }

        private void Update()
        {
            _text.text = GetProgress().ToString("0.##") + "%";
        }

        public void SetBaseTexture(RenderTexture texture)
        {
            _baseTexture = texture;
            _shader.SetTexture(_mainKernelHandle, "baseImage", _baseTexture);
            _shader.SetTexture(_pixelsKernelHandle, "baseImage", _baseTexture);
        }

        public void SetTexture(RenderTexture texture)
        {
            _texture = texture;
            _shader.SetTexture(_mainKernelHandle, "image", _texture);
            //_shader.SetTexture(_pixelsKernelHandle, "image", _texture);
        }

        public void SetTargetColor(Color value)
        {
            _targetColor = value;
            _shader.SetVector("color", _targetColor);
        }

        public float GetProgress()
        {
            if (_baseTexture == null || _texture == null)
            {
                return -1;
            }

            _data[0] = 0;
            _data[1] = 0;
            _buffer.SetData(_data);
            _shader.Dispatch(_mainKernelHandle, _texture.width / 8, _texture.height / 8, 1);
            _shader.Dispatch(_pixelsKernelHandle, _texture.width / 8, _texture.height / 8, 1);
            _buffer.GetData(_data);

            var percent = 0f;
            if (_data[1] != 0)
            {
                percent = _data[0] * 1.0f / _data[1] * 100.0f;
            }

            return percent;
        }
    }
}
