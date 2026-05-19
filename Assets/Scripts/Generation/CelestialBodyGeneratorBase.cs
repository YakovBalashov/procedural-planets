using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public abstract class CelestialBodyGeneratorBase : MeshGenerator
    {
        public abstract void GenerateBodyData();
        public abstract void UpdateSurface(int seed);
        
        public abstract CelestialBodyData GetBodyData();
        
        public abstract Mesh GenerateMeshOnGPU(int resolution);
        
        public abstract void UpdateMaterial();
    }
}
