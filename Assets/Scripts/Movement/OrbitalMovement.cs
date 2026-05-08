using System;
using ProceduralPlanets.Generation;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace ProceduralPlanets.Movement
{
    public class OrbitalMovement : MonoBehaviour
    {
        [SerializeField] private float radiusX = 5f;
        [SerializeField] private float radiusZ = 5f;
        [SerializeField] private float speedInDegreesPerSecond = 1f;
        [SerializeField] private Vector3 rotation;

        [Range(10, 360)]
        [SerializeField] private int segmentNumber = 100;

        [field: FormerlySerializedAs("<color>k__BackingField")] [field: SerializeField]
        public Color Color { get; private set; } = Color.cyan;

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

        public void SetParameters(OrbitParameters parameters)
        {
            radiusX = parameters.MainRadius;
            radiusZ = parameters.MainRadius * parameters.RadiusRatio;
            rotation.y = parameters.Rotation;
            rotation.z = parameters.Inclination;
            speedInDegreesPerSecond = parameters.SpeedInDegreesPerSecond;
            Initialize();
        }

        public void ChangeRadius(float deltaRadius)
        {
            radiusX += deltaRadius;
            radiusZ += deltaRadius;
            Initialize();
        }

        public void ChangeSpeed(float deltaSpeed)
        {
            speedInDegreesPerSecond += deltaSpeed;
        }

        public void ChangeInclination(float deltaInclination)
        {
            rotation.z += deltaInclination;
        }

        public Vector3 GetPositionAfterTime(float time)
        {
            float futureAngle = _currentAngle + speedInDegreesPerSecond * Mathf.Deg2Rad * time;
            futureAngle %= 2 * Mathf.PI;

            var rotationQuaternion = Quaternion.Euler(rotation);
            Vector3 localRotatedPoint = rotationQuaternion * GetLocalPointOnEllipse(futureAngle);

            return localRotatedPoint;
        }

        private void Initialize()
        {
            _centerToFocusDistance = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(radiusX, 2) - Mathf.Pow(radiusZ, 2)));
            _mainAxis = (radiusX >= radiusZ) ? Vector3.right : Vector3.forward;
        }

        private void FixedUpdate()
        {
            _currentAngle += speedInDegreesPerSecond * Mathf.Deg2Rad * Time.fixedDeltaTime * SystemGenerator.TimeMultiplier;
            _currentAngle %= 2 * Mathf.PI;

            MoveBodyToAngle(_currentAngle);
        }

        private void MoveBodyToAngle(float angle)
        {
            if (!transform.parent) return;

            var rotationQuaternion = Quaternion.Euler(rotation);

            Vector3 localRotatedPoint = rotationQuaternion * GetLocalPointOnEllipse(angle);

            transform.position = transform.parent.TransformPoint(localRotatedPoint);
        }

        private Vector3 GetLocalPointOnEllipse(float angle)
        {
            return new Vector3(radiusX * Mathf.Cos(angle), 0f, radiusZ * Mathf.Sin(angle)) +
                   _mainAxis * _centerToFocusDistance;
        }

        public void MoveToStartingPosition(Random random)
        {
            _currentAngle = (float)(random.NextDouble() * 2 * Math.PI);
            MoveBodyToAngle(_currentAngle);
        }

        public void CalculateAngleFromPositionForPerfectOrbit()
        {
            if (!transform.parent) return;

            var angle = Mathf.Atan2(transform.localPosition.z, transform.localPosition.x);
            _currentAngle = angle % (2 * Mathf.PI);
        }
        
        public void SetCircularOrbitFromCurrentPosition(float orbitalVelocity)
        {
            if (!transform.parent) return;

            var localPos = transform.localPosition;
            var radius = localPos.magnitude;
            
            radiusX = radius;
            radiusZ = radius;
            speedInDegreesPerSecond = orbitalVelocity;

            _currentAngle = 0f;

            var inclination = Mathf.Asin(localPos.y / radius) * Mathf.Rad2Deg;

            var yaw = Mathf.Atan2(-localPos.z, localPos.x) * Mathf.Rad2Deg;

            rotation = new Vector3(0f, yaw, inclination);

            Initialize();

            MoveBodyToAngle(_currentAngle);
        }

        private void OnDrawGizmos()
        {
            if (transform.parent == null) return;

            Gizmos.color = Color;

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