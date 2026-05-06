using UnityEngine;

namespace ProceduralPlanets.Movement
{
    [System.Serializable]
    public struct OrbitParameters
    {
        [field: SerializeField] public float MainRadius {get; private set; }
        [field: SerializeField] public float RadiusRatio {get; private set; }
        [field: SerializeField] public float Inclination {get; private set; }
        [field: SerializeField] public float Rotation {get; private set; }
        [field: SerializeField] public float SpeedInDegreesPerSecond {get; private set; }

        public OrbitParameters(float mainRadius, float radiusRatio, float inclination, float rotation,
            float speedInDegreesPerSecond)
        {
            MainRadius = mainRadius;
            RadiusRatio = radiusRatio;
            Inclination = inclination;
            Rotation = rotation;
            SpeedInDegreesPerSecond = speedInDegreesPerSecond;
        }
    }
}