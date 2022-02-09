using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 Divide(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
    }
}
