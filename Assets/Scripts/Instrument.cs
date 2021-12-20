using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    public abstract class Instrument : MonoBehaviour
    {
        public abstract bool IsNeedDisablingRotation();
        public abstract void Enable();
        public abstract void Disable();
    }
}
