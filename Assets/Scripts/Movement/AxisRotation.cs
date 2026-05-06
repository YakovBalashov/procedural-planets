using System;
using ProceduralPlanets.Generation;
using UnityEngine;

namespace ProceduralPlanets.Movement
{
    public class AxisRotation : MonoBehaviour
    {
        [SerializeField] private Vector3 axis;
        [SerializeField] private float speedInDegreesPerSecond = 1f;

        private Quaternion _axisRotation;
        private float _currentAngle;
        
        private void Awake()
        {
            AlignAxis();
        }

        private void OnValidate()
        {
            AlignAxis();
        }
        
        public void SetParameters(Vector3 newAxis, float newSpeedInDegreesPerSecond)
        {
            axis = newAxis;
            speedInDegreesPerSecond = newSpeedInDegreesPerSecond;
            AlignAxis();
        }
        
        private void AlignAxis()
        {
            _axisRotation = Quaternion.FromToRotation(Vector3.up, axis.normalized);
        }

        private void FixedUpdate()
        {
            _currentAngle = Mathf.Repeat(speedInDegreesPerSecond * Time.fixedTime * SystemGenerator.TimeMultiplier, 360f);
            transform.rotation = _axisRotation * Quaternion.Euler(0f, _currentAngle, 0f);
        }
    }
}