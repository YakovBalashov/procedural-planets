using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public abstract class CelestialBodyGeneratorBase : MonoBehaviour
    {
        public abstract void GenerateBodyData();
        public abstract void UpdateSurface();
        
        public abstract CelestialBodyData GetBodyData();
        
        public abstract Mesh GenerateMeshOnGPU(int resolution);
        
        public abstract void UpdateMaterial();
    }
}
