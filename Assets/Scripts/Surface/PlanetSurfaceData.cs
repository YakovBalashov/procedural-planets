using UnityEngine;

namespace ProceduralPlanets.Surface
{
    [CreateAssetMenu(fileName = "PlanetSurfaceData", menuName = "Planet Data/PlanetSurfaceData")]
    public class PlanetSurfaceData : ScriptableObject
    {
        [Range(1.0f, 100f)]
        public float radius;
    }
}
