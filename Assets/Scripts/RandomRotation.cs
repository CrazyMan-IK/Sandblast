using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using URandom = UnityEngine.Random;

namespace Sandblast
{
    public class RandomRotation : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 140;
        [SerializeField] private float _accelerationSpeed = 2;
        [SerializeField] private float _rotationTimer = 2;

        private Vector3 _targetRotation = Vector3.zero;
        private Vector3 _rotation = Vector3.zero;
        private float _currentTime = 0;

        private void Awake()
        {
            _targetRotation = URandom.onUnitSphere;
            _rotation = URandom.onUnitSphere;
        }

        private void Update()
        {
            _currentTime += Time.unscaledDeltaTime;
            if (_currentTime >= _rotationTimer)
            {
                _currentTime = 0;
                _targetRotation = URandom.onUnitSphere;
            }

            //transform.rotation = Quaternion.Slerp(transform.rotation, _rotation, _rotationSpeed * Time.deltaTime);
            _rotation = Vector3.Slerp(_rotation, _targetRotation, _accelerationSpeed * Time.unscaledDeltaTime);
            transform.Rotate(_rotation * _rotationSpeed * Time.unscaledDeltaTime);
        }
    }
}
