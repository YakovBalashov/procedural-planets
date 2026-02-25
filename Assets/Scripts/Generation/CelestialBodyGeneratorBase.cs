using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public abstract class CelestialBodyGeneratorBase : MonoBehaviour
    {
        public abstract void GenerateBodyData();
        public abstract void UpdateSurface();
        
        public abstract CelestialBodyData GetBodyData();
    }
}
