using UnityEngine;

namespace ProceduralPlanets
{
    public struct OrbitParameters
    {
        public readonly float MainRadius;
        public readonly float RadiusRatio;
        public readonly float Inclination;
        public readonly float Rotation;
        public readonly float SpeedInDegreesPerSecond;

        public OrbitParameters(float mainRadius, float radiusRatio, float inclination, float rotation, float speedInDegreesPerSecond)
        {
            MainRadius = mainRadius;
            RadiusRatio = radiusRatio;
            Inclination = inclination;
            Rotation = rotation;
            SpeedInDegreesPerSecond = speedInDegreesPerSecond;
        }
    }
}
