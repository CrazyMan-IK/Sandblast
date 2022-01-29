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

        public PaintableTexture(Color clearColor, int width, int height, string id, Shader sPaintInUV, Mesh mToDraw, Shader fixIlsandEdgesShader, Texture markedIlsandes) : this(clearColor, width, height, id, sPaintInUV, mToDraw, fixIlsandEdgesShader, markedIlsandes, null, null)
        {

        }

        public PaintableTexture(Color clearColor, int width, int height, string id, Shader sPaintInUV, Mesh mToDraw, Shader fixIlsandEdgesShader, Texture markedIlsandes, RenderTexture runTimeTexture, RenderTexture paintedTexture)
        {
            _id = id;

            _runTimeTexture = runTimeTexture ?? new RenderTexture(width, height, 0)
            {
                anisoLevel = 0,
                useMipMap = false,
                filterMode = FilterMode.Bilinear
            };

            _runTimeTexture.name = $"Runtime{id}";

            _paintedTexture = paintedTexture ?? new RenderTexture(width, height, 0)
            {
                anisoLevel = 0,
                useMipMap = false,
                filterMode = FilterMode.Bilinear
            };

            _paintedTexture.name = $"Painted{id}";
            
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

        public void BlitWithTexture(Texture tex, Shader shader, Color color, Texture uvMask)
        {
            BlitWithTexture(tex, shader, color, uvMask, false).MoveNext();
        }

        public void ReplacePaintShader(Shader newShader)
        {
            _paintInUV.shader = newShader;
            _paintInUV.SetTexture("_MainTex", _paintedTexture);
        }

        public IEnumerator BlitWithTexture(Texture tex, Shader shader, Color color, Texture uvMask, bool fixIlsands = false)
        {
            var temp1 = RenderTexture.GetTemporary(_runTimeTexture.descriptor);
            var temp2 = RenderTexture.GetTemporary(_paintedTexture.descriptor);

            var mat = new Material(shader);
            mat.SetTexture("_SecondTex", tex);
            mat.SetTexture("_UVMask", uvMask);
            mat.color = color; //Color.white; // - Color.black;

            //UnityEditorInternal.RenderDoc.BeginCaptureRenderDoc(UnityEditor.EditorWindow.focusedWindow);
            Graphics.Blit(_runTimeTexture, temp1, mat);
            Graphics.Blit(temp1, _runTimeTexture, mat);

            Graphics.Blit(_paintedTexture, temp2, mat);
            Graphics.Blit(temp2, _paintedTexture, mat);
            //UnityEditorInternal.RenderDoc.EndCaptureRenderDoc(UnityEditor.EditorWindow.focusedWindow);

            RenderTexture.ReleaseTemporary(temp1);
            RenderTexture.ReleaseTemporary(temp2);

            if (fixIlsands)
            {
                for (int i = 0; i < 5; i++)
                {
                    Graphics.Blit(_runTimeTexture, _fixedIlsands, _fixedEdges);
                    Graphics.Blit(_fixedIlsands, _runTimeTexture);
                    Graphics.Blit(_runTimeTexture, _paintedTexture);

                    yield return null;
                }
            }
        }

        public void SetActiveTexture(Camera camera)
        {
            camera.AddCommandBuffer(CameraEvent.AfterDepthTexture, _buffer);
        }

        public void SetInactiveTexture(Camera camera)
        {
            camera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, _buffer);
        }

        public void UpdateShaderParameters(Matrix4x4 localToWorld, Texture mask)
        {
            _paintInUV.SetMatrix("mesh_Object2World", localToWorld);
            _paintInUV.SetTexture("_UVMask", mask);
        }
    }
}
