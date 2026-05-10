using System;
using ProceduralPlanets.Generation;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace ProceduralPlanets.Movement
{
    public class OrbitalMovement : MonoBehaviour
    {
        [SerializeField] protected float radiusX = 5f;
        [SerializeField] protected float radiusZ = 5f;
        [SerializeField] protected float speedInDegreesPerSecond = 1f;
        [SerializeField] protected Vector3 rotation;

        [Range(10, 360)]
        [SerializeField] private int segmentNumber = 100;

        [field: FormerlySerializedAs("<color>k__BackingField")] [field: SerializeField]
        public Color Color { get; private set; } = Color.cyan;

        private float _centerToFocusDistance;
        private Vector3 _mainAxis;

        protected float CurrentAngle = 0f;

        private void OnValidate()
        {
            Initialize();
        }

        private void Awake()
        {
            Initialize();
        }

        public virtual void SetParameters(OrbitParameters parameters)
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
            float futureAngle = CurrentAngle + speedInDegreesPerSecond * Mathf.Deg2Rad * time;
            futureAngle %= 2 * Mathf.PI;

            var rotationQuaternion = Quaternion.Euler(rotation);
            Vector3 localRotatedPoint = rotationQuaternion * GetLocalPointOnEllipse(futureAngle);

            return localRotatedPoint;
        }

        protected void Initialize()
        {
            _centerToFocusDistance = Mathf.Sqrt(Mathf.Abs(Mathf.Pow(radiusX, 2) - Mathf.Pow(radiusZ, 2)));
            _mainAxis = (radiusX >= radiusZ) ? Vector3.right : Vector3.forward;
        }

        private void Update()
        {
            CurrentAngle += speedInDegreesPerSecond * Mathf.Deg2Rad * Time.deltaTime * SystemGenerator.TimeMultiplier;
            CurrentAngle %= 2 * Mathf.PI;

            MoveBodyToAngle(CurrentAngle);
        }

        protected virtual void MoveBodyToAngle(float angle)
        {
            if (!transform.parent) return;

            var rotationQuaternion = Quaternion.Euler(rotation);

            Vector3 localRotatedPoint = rotationQuaternion * GetLocalPointOnEllipse(angle);
            
            transform.localPosition = localRotatedPoint;
        }

        protected Vector3 GetLocalPointOnEllipse(float angle)
        {
            return new Vector3(radiusX * Mathf.Cos(angle), 0f, radiusZ * Mathf.Sin(angle)) +
                   _mainAxis * _centerToFocusDistance;
        }

        public void MoveToStartingPosition(Random random)
        {
            CurrentAngle = (float)(random.NextDouble() * 2 * Math.PI);
            MoveBodyToAngle(CurrentAngle);
        }

        public void CalculateAngleFromPositionForPerfectOrbit()
        {
            if (!transform.parent) return;

            var angle = Mathf.Atan2(transform.localPosition.z, transform.localPosition.x);
            CurrentAngle = angle % (2 * Mathf.PI);
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

        public void ZeroSpeed()
        {
            speedInDegreesPerSecond = 0f;
        }
    }
}