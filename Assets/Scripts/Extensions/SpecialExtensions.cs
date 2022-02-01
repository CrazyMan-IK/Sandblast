using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    public static class SpecialExtensions
    {
        public static RaycastHit GetClosestHit(this RaycastHit[] hits, int count)
        {
            if (count < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var closest = hits[0];

            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];

                if (hit.distance < closest.distance)
                {
                    closest = hit;
                }
            }

            return closest;
        }
    }
}
