using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Sandblast
{
    public class FilledColor : MonoBehaviour
    {
        [SerializeField] private ComputeShader _shader = null;
        [SerializeField] private RenderTexture _baseTexture = null;
        [SerializeField] private RenderTexture _texture = null;
        [SerializeField] private Color _targetColor = Color.white;
        [SerializeField] private Image _progress = null;

        private ComputeBuffer _buffer = null;
        private readonly uint[] _data = new uint[2] { 0, 0 };

        private int _mainKernelHandle = -1;
        //private int _prevHashCode = 0;
        private float _prevProgress = 0;

        private void Awake()
        {
            _mainKernelHandle = _shader.FindKernel("CSMain");
            _buffer = new ComputeBuffer(2, sizeof(uint));

            _shader.SetBuffer(_mainKernelHandle, "result", _buffer);

            StartCoroutine(UpdateProgress());
        }

        private void OnDestroy()
        {
            _buffer.Dispose();
        }

        private IEnumerator UpdateProgress()
        {
            var wait = new WaitForSeconds(0.1f);
            while (true)
            {
                _progress.fillAmount = GetProgress();
                yield return wait;
            }
        }

        public void SetBaseTexture(RenderTexture texture)
        {
            if (_baseTexture == texture)
            {
                return;
            }

            _baseTexture = texture;

            _shader.SetTexture(_mainKernelHandle, "baseImage", _baseTexture);
        }

        public void SetTexture(RenderTexture texture)
        {
            if (_texture == texture)
            {
                return;
            }

            _texture = texture;
            _shader.SetTexture(_mainKernelHandle, "image", _texture);
        }

        public void SetTargetColor(Color value)
        {
            if (_targetColor == value)
            {
                return;
            }

            _targetColor = value;
            _shader.SetVector("color", _targetColor);
        }

        public bool IsFilled()
        {
            return GetProgress() >= 1;
        }

        public float GetProgress()
        {
            if (_baseTexture == null || _texture == null)
            {
                return -1;
            }

            /*var currentHashCode = GetCurrentHashCode();
            if (_prevHashCode == currentHashCode)
            {
                return _prevProgress;
            }

            _prevHashCode = currentHashCode;*/

            _data[0] = 0;
            _data[1] = 0;
            _buffer.SetData(_data);
            _shader.Dispatch(_mainKernelHandle, _texture.width / 8, _texture.height / 8, 1);
            _buffer.GetData(_data);

            if (_data[1] != 0)
            {
                _prevProgress = (_data[0] * 1.0f / _data[1]) * 1.05f;
            }

            return _prevProgress;
        }

        /*private int GetCurrentHashCode()
        {
            unchecked
            {
                var h1 = GetTextureHashCode(_baseTexture); //_baseTexture.GetNativeTexturePtr().GetHashCode(); //_baseTexture.imageContentsHash.GetHashCode();
                var h2 = GetTextureHashCode(_texture); //_texture.GetNativeTexturePtr().GetHashCode(); //_texture.imageContentsHash.GetHashCode();
                var h3 = _targetColor.GetHashCode();

                return h1 + h2 + h3;
            }
        }

        private int GetTextureHashCode(Texture texture)
        {
            var ptr = texture.GetNativeTexturePtr();

            *//*var hash = 0;
            for (int i = 0; i < texture.width * texture.height; i++)
            {
                var p = (ptr + i).ToInt32();
                Hash128.Compute();
            }*//*

            Hash128 hash;
            unsafe
            {
                var width = (ulong)texture.width;
                var height = (ulong)texture.height;
                hash = Hash128.Compute(ptr.ToPointer(), width * height);
            }
            return hash.GetHashCode();
        }*/
    }
}
