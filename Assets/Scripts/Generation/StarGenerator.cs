using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(Light))]
    public class StarGenerator : CelestialBodyGenerator<StarData, StarType>
    {
        private Light _pointLight;
        protected override void Initialize()
        {
            base.Initialize();
            if (_pointLight) return;
            _pointLight = GetComponent<Light>();
        }

        protected override void GenerateMesh()
        {
            var mesh = IcoSphereGenerator.Generate(subdivisionLevel, BodyData.Radius);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            MeshFilter.sharedMesh = mesh;
        }

        public override void UpdateSurface()
        {
            base.UpdateSurface();
            _pointLight.color = BodyData.SurfaceMaterial.color;
        }
    }
}