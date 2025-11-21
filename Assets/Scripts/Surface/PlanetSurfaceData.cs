using UnityEngine;

namespace ProceduralPlanets.Surface
{
    [CreateAssetMenu(fileName = "PlanetSurfaceData", menuName = "Planet Data/PlanetSurfaceData")]
    public class PlanetSurfaceData : ScriptableObject
    {
        [Range(0.001f, 1000f)]
        public float radius;
    }
}
