using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    public class PaintablesHolder : MonoBehaviour
    {
        private readonly Dictionary<string, PaintableTexture> _textures = new Dictionary<string, PaintableTexture>();

        public PaintableTexture GetTexture(string name)
        {
            return _textures[name];
        }

        public bool TryAddTexture(string name, PaintableTexture texture)
        {
            if (_textures.ContainsKey(name))
            {
                return false;
            }

            _textures[name] = texture;
            return true;
        }
    }
}
