using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [System.Serializable]
    public struct SatelliteParameters
    {
        [field: SerializeField] public Vector2 NumberRange { get; private set; }
        [field: SerializeField] public Vector2 OrbitRadiusRange { get; private set; }
        [field: SerializeField] public Vector2 DistanceBetweenSatellitesRange { get; private set; }
    }
}
