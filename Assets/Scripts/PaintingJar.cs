using Sandblast.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    [RequireComponent(typeof(MeshRenderer))]
    public class PaintingJar : MonoBehaviour
    {
        private MeshRenderer _renderer = null;

        public BrushStage Stage { get; private set; }
        public Bounds Bounds => _renderer.bounds;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
        }

        public void Init(BrushStage stage)
        {
            Stage = stage;

            _renderer.material.SetColor("_Color", stage.Color);
            Color.RGBToHSV(stage.Color, out var H, out var S, out var V);
            V -= 0.25f;
            _renderer.material.SetColor("_ColorDim", Color.HSVToRGB(H, S, V));
        }
    }
}
