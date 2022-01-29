using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast.Extensions
{
    public static class MathExtensions
    {
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
        }
    }
}
