using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast.Models
{
    [Serializable]
    public class InstrumentInformation
    {
        [SerializeField, Range(0.001f, 1)] private float _maxFill = 1;
        [SerializeField] private Instrument _instrument = null;
        [SerializeField] private Color _targetColor = Color.yellow;
        [SerializeField] private Texture _uvMask = null;

        public float MaxFill => _maxFill;
        public Instrument Instrument => _instrument;
        public Color TargetColor => _targetColor;
        public Texture UVMask => _uvMask;
    }
}
