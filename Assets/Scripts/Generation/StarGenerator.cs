using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(Light))]
    public class StarGenerator : CelestialBodyGenerator<StarData, StarType>
    {
        protected override void GenerateMesh()
        {
            var mesh = CubeSphereGenerator.Generate(resolution, BodyData.Radius);
            MeshFilter.sharedMesh = mesh;
        }
    }
}