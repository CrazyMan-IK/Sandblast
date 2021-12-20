using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast.Models
{
    [CreateAssetMenu(fileName = "New LevelInformation", menuName = "Sandblast/Level Information", order = 50)]
    public class LevelInformation : ScriptableObject
    {
        [SerializeField] private Mesh _mesh = null;

        public Mesh Mesh => _mesh;
    }
}
