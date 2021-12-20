using Sandblast.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast
{
    public class Level : MonoBehaviour
    {
        [SerializeField] private LevelInformation _level = null;
        [SerializeField] private MeshFilter _target = null;

        private void Awake()
        {
            _target.mesh = _level.Mesh;

            if (_target.TryGetComponent(out MeshCollider collider))
            {
                collider.sharedMesh = _level.Mesh;
            }
        }
    }
}
