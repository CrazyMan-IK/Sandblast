using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Sandblast
{

    [System.Serializable]
    public class PaintableTexture
    {
        private readonly string _id;
        private readonly RenderTexture _runTimeTexture;
        private readonly RenderTexture _paintedTexture;

        private readonly CommandBuffer _buffer;

        private readonly Material _paintInUV;
        private readonly Material _fixedEdges;
        private readonly RenderTexture _fixedIlsands;

        public string Id => _id;
        public RenderTexture RuntimeTexture => _runTimeTexture;
        public RenderTexture PaintedTexture => _paintedTexture;

        public PaintableTexture(Color clearColor, int width, int height, string id, Shader sPaintInUV, Mesh mToDraw, Shader fixIlsandEdgesShader, RenderTexture markedIlsandes) : this(clearColor, width, height, id, sPaintInUV, mToDraw, fixIlsandEdgesShader, markedIlsandes, null, null)
        {

        }

        public PaintableTexture(Color clearColor, int width, int height, string id, Shader sPaintInUV, Mesh mToDraw, Shader fixIlsandEdgesShader, RenderTexture markedIlsandes, RenderTexture runTimeTexture, RenderTexture paintedTexture)
        {
            _id = id;

            _runTimeTexture = runTimeTexture ?? new RenderTexture(width, height, 0)
            {
                anisoLevel = 0,
                useMipMap = false,
                filterMode = FilterMode.Bilinear
            };

            _paintedTexture = paintedTexture ?? new RenderTexture(width, height, 0)
            {
                anisoLevel = 0,
                useMipMap = false,
                filterMode = FilterMode.Bilinear
            };
            _fixedIlsands = new RenderTexture(_paintedTexture.descriptor);

            Graphics.SetRenderTarget(_runTimeTexture);
            GL.Clear(false, true, clearColor);
            Graphics.SetRenderTarget(_paintedTexture);
            GL.Clear(false, true, clearColor);
            _paintInUV = new Material(sPaintInUV);
            if (!_paintInUV.SetPass(0))
            {
                Debug.LogError("Invalid Shader Pass: ");
            }
            _paintInUV.SetTexture("_MainTex", _paintedTexture);

            _fixedEdges = new Material(fixIlsandEdgesShader);
            _fixedEdges.SetTexture("_IlsandMap", markedIlsandes);
            _fixedEdges.SetTexture("_MainTex", _paintedTexture);

            _buffer = new CommandBuffer();
            _buffer.name = "TexturePainting" + id;
            _buffer.SetRenderTarget(_runTimeTexture);
            _buffer.DrawMesh(mToDraw, Matrix4x4.identity, _paintInUV);

            _buffer.Blit(_runTimeTexture, _fixedIlsands, _fixedEdges);
            _buffer.Blit(_fixedIlsands, _runTimeTexture);
            _buffer.Blit(_runTimeTexture, _paintedTexture);
        }

        public void BlitWithTexture(Texture tex, Shader shader)
        {
            var mat = new Material(shader);
            mat.mainTexture = tex;

            Graphics.Blit(tex, _paintedTexture, mat);
            //_buffer.Blit(tex, _paintedTexture, mat);
        }

        public void SetActiveTexture(Camera camera)
        {
            camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, _buffer);
        }

        public void SetInactiveTexture(Camera camera)
        {
            camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, _buffer);
        }

        public void UpdateShaderParameters(Matrix4x4 localToWorld)
        {
            _paintInUV.SetMatrix("mesh_Object2World", localToWorld);
        }
    }
}
