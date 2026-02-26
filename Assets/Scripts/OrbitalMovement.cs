using System;
using UnityEngine;

namespace ProceduralPlanets
{
    public class OrbitalMovement : MonoBehaviour
    {
        [SerializeField] private float radiusX = 5f;
        [SerializeField] private float radiusZ = 5f;
        [SerializeField] private float speedInDegreesPerSecond = 1f;
        [SerializeField] private Vector3 rotation;

        [Range(10, 360)]
        [SerializeField] private int segmentNumber = 100;
        [SerializeField] private Color color = Color.cyan;

        private float _centerToFocusDistance;
        private Vector3 _mainAxis;
        
        private float _currentAngle = 0f;

        private void OnValidate()
        {
            Initialize();
        }

        private void Awake()
        {
            Initialize();
        }
        
        public void SetParameters(float newRadiusX, float newRadiusZ, float newSpeedInDegreesPerSecond)
        {
            radiusX = newRadiusX;
            radiusZ = newRadiusZ;
            speedInDegreesPerSecond = newSpeedInDegreesPerSecond;
            Initialize();
        }

        private void Initialize()
        {
            _centerToFocusDistance = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(radiusX, 2) + Mathf.Pow(radiusZ, 2))) / 2f;
            _mainAxis = (radiusX >= radiusZ) ? Vector3.right : Vector3.forward;
        }

        private void FixedUpdate()
        {
            _currentAngle += speedInDegreesPerSecond * Mathf.Deg2Rad * Time.fixedDeltaTime;
            _currentAngle %= 2 * Mathf.PI;
            
            var rotationQuaternion = Quaternion.Euler(rotation);
            
            Vector3 localRotatedPoint = rotationQuaternion * GetLocalPointOnEllipse(_currentAngle);
            
            transform.position = transform.parent.TransformPoint(localRotatedPoint);
        }

        private Vector3 GetLocalPointOnEllipse(float angle)
        {
            return new Vector3(radiusX * Mathf.Cos(angle), 0f, radiusZ * Mathf.Sin(angle)) + _mainAxis * _centerToFocusDistance;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = color;

            float angleStep = (2 * Mathf.PI) / segmentNumber;
            
            var rotationQuaternion = Quaternion.Euler(rotation);

            Vector3 previousPoint = transform.parent.TransformPoint(rotationQuaternion * GetLocalPointOnEllipse(0f));

            for (var i = 1; i <= segmentNumber; i++)
            {
                float angle = i * angleStep;
                Vector3 point = transform.parent.TransformPoint(rotationQuaternion * GetLocalPointOnEllipse(angle));

                Gizmos.DrawLine(previousPoint, point);

                previousPoint = point;
            }
        }
    }
}