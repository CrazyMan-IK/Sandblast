using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sandblast.UI;
using System;
using Sandblast.Extensions;

namespace Sandblast
{
    public class FilledColor : MonoBehaviour
    {
        [SerializeField] private ComputeShader _shader = null;
        [SerializeField] private RenderTexture _baseTexture = null;
        [SerializeField] private RenderTexture _texture = null;
        [SerializeField] private Texture _uvMask = null;
        [SerializeField] private Color _targetColor = Color.white;
        [SerializeField] private bool _useTargetColor = false;
        [SerializeField] private ProgressBar _progress = null;
        [SerializeField] private RawImage _output1 = null;
        [SerializeField] private RawImage _output2 = null;
        [SerializeField] private RawImage _output3 = null;

        private ComputeBuffer _buffer = null;
        private readonly uint[] _data = new uint[2] { 0, 0 };

        private int _mainKernelHandle = -1;
        //private int _prevHashCode = 0;
        private float _prevProgress = 0;
        private float _maxFill = 0;
        private bool _isManual = false;
        private float _manualValue = 0;

        //private Texture2D _textureBuffer = null;

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
                //UnityEditorInternal.RenderDoc.BeginCaptureRenderDoc(UnityEditor.EditorWindow.focusedWindow);

                _progress.SetValue(GetProgress());

                //UnityEditorInternal.RenderDoc.EndCaptureRenderDoc(UnityEditor.EditorWindow.focusedWindow);
                yield return wait;
            }
        }

        public void EnableManualMode()
        {
            _isManual = true;
        }

        public void DisableManualMode()
        {
            _isManual = false;
        }

        public void SetManualValue(float value)
        {
            _manualValue = value;
        }

        public void SetBaseTexture(RenderTexture texture)
        {
            if (_baseTexture == texture || texture == null)
            {
                return;
            }

            _output1.texture = texture;
            _baseTexture = texture;
            //_textureBuffer = new Texture2D(texture.width, texture.height);
            _shader.SetTexture(_mainKernelHandle, "baseImage", _baseTexture);
        }

        public void SetTexture(RenderTexture texture)
        {
            if (_texture == texture || texture == null)
            {
                return;
            }

            _output2.texture = texture;
            _texture = texture;
            _shader.SetTexture(_mainKernelHandle, "image", _texture);
        }

        public void SetUVMask(Texture texture)
        {
            if (_uvMask == texture || texture == null)
            {
                return;
            }

            _output3.texture = texture;
            _uvMask = texture;
            _shader.SetTexture(_mainKernelHandle, "mask", _uvMask);
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

        public void SetTargetColorUsage(bool value)
        {
            if (_useTargetColor == value)
            {
                return;
            }

            _useTargetColor = value;
            _shader.SetBool("useColor", _useTargetColor);
        }

        public void SetMaxFill(float value)
        {
            if (value < 0 || value > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _maxFill = value;
        }

        public bool IsFilled()
        {
            return GetProgress() >= 1;
        }

        public float GetProgress()
        {
            if (_isManual)
            {
                return _manualValue * (1 - _maxFill + 1);
            }

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
                _prevProgress = (_data[0] * 1.0f / _data[1]) * (1 - _maxFill + 1); //1.05f;
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
        }*/

        /*private int GetCurrentHashCode()
        {
            unchecked
            {
                RenderTexture.active = _baseTexture;
                _textureBuffer.ReadPixels(new Rect(0, 0, _textureBuffer.width, _textureBuffer.height), 0, 0);
                var h1 = Hash128.Compute(_textureBuffer.GetRawTextureData()).GetHashCode();
                //var h1 = _textureBuffer.GetRawTextureData().GetHashCode();

                RenderTexture.active = _texture;
                _textureBuffer.ReadPixels(new Rect(0, 0, _textureBuffer.width, _textureBuffer.height), 0, 0);
                var h2 = Hash128.Compute(_textureBuffer.GetRawTextureData()).GetHashCode();
                //var h2 = _textureBuffer.GetRawTextureData().GetHashCode();
                
                var h3 = _targetColor.GetHashCode();

                return h1 + h2 + h3;
            }
        }*/
    }
}
