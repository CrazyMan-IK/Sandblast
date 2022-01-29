using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast.Models
{
    [CreateAssetMenu(fileName = "New LevelInformation", menuName = "Sandblast/Level Information", order = 50)]
    public class LevelInformation : ScriptableObject
    {
        [SerializeField] private Texture _baseTexture = null;
        [SerializeField] private Mesh _mesh = null;
        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private Vector3 _scale = Vector3.one;
        [SerializeField] private Vector3 _rotation = Vector3.zero;
        [SerializeField] private List<InstrumentInformation> _instruments = new List<InstrumentInformation>();

        public Texture BaseTexture => _baseTexture;
        public Mesh Mesh => _mesh;
        public Vector3 Offset => _offset;
        public Vector3 Scale => _scale;
        public Vector3 Rotation => _rotation;
        public IReadOnlyList<InstrumentInformation> Instruments => _instruments;
    }
}
