using UnityEngine;

namespace ProceduralPlanets.Movement
{
    public class PlayerOrbitalMovement : OrbitalMovement
    {
        private Transform _parent;
        protected override void MoveBodyToAngle(float angle)
        {
            if (!_parent) return;

            var rotationQuaternion = Quaternion.Euler(rotation);

            Vector3 localRotatedPoint = rotationQuaternion * GetLocalPointOnEllipse(angle);
            
            transform.position = _parent.position + localRotatedPoint;
        }

        public override void SetParameters(OrbitParameters parameters)
        {
            base.SetParameters(parameters);
            _parent = transform.parent;
            transform.SetParent(null);
        }

        public void SetCircularOrbitFromCurrentPosition(float orbitalVelocity)
        {
            if (!transform.parent) return;
            
            _parent = transform.parent;
            transform.SetParent(null);

            var localPos = _parent.InverseTransformPoint(transform.position);
            var radius = localPos.magnitude;
            
            radiusX = radius;
            radiusZ = radius;
            speedInDegreesPerSecond = orbitalVelocity;

            CurrentAngle = 0f;

            var inclination = Mathf.Asin(localPos.y / radius) * Mathf.Rad2Deg;

            var yaw = Mathf.Atan2(-localPos.z, localPos.x) * Mathf.Rad2Deg;

            rotation = new Vector3(0f, yaw, inclination);

            Initialize();

            MoveBodyToAngle(CurrentAngle);
        }
    }
}
