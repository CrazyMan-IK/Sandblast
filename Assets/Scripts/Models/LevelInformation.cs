using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sandblast.Models
{
    [CreateAssetMenu(fileName = "New LevelInformation", menuName = "Sandblast/Level Information", order = 50)]
    public class LevelInformation : ScriptableObject
    {
        [SerializeField] private Mesh _mesh = null;
        [SerializeField] private Vector3 _offset = Vector3.zero;
        [SerializeField] private Vector3 _scale = Vector3.one;
        [SerializeField] private Color _targetColor = Color.yellow;
        [SerializeField] private int _availableInstrumentsCount = 0;

        public Mesh Mesh => _mesh;
        public Vector3 Offset => _offset;
        public Vector3 Scale => _scale;
        public Color TargetColor => _targetColor;
        public int AvailableInstrumentsCount => _availableInstrumentsCount;
    }
}
