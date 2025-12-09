using System.Linq;
using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.Noise;
using ProceduralPlanets.Surface;
using UnityEngine;

namespace ProceduralPlanets
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetSurface : MonoBehaviour
    {
        private static readonly int Min = Shader.PropertyToID("_Min");
        private static readonly int Max = Shader.PropertyToID("_Max");

        [Header("Mesh")] [SerializeField, Range(0, 6)]
        private int subdivisionLevel;

        [Header("Surface")] public PlanetSurfaceData surfaceData;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private void OnValidate()
        {
            UpdateSurface();
        }

        public void UpdateSurface()
        {
            Initialize();
            GenerateMesh();
        }

        private void Initialize()
        {
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
            if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();
            if (surfaceData && surfaceData.PlanetMaterial)
                _meshRenderer.sharedMaterial = surfaceData.PlanetMaterial; 
        }

        private void GenerateMesh()
        {
            var mesh = IcoSphereGenerator.Generate(subdivisionLevel, surfaceData.Radius);

            var noiseGenerators = (from noiseSetting in surfaceData.NoiseSettings
                where noiseSetting.Enabled
                select new NoiseGenerator(noiseSetting)).ToList();
            
            var minMaxElevations = new MinMax();
            var vertices = mesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];

                var elevation = noiseGenerators.Sum(noiseGenerator => noiseGenerator.Evaluate(vertex.normalized));

                var distanceFromCenter = surfaceData.Radius * (1 + elevation);

                minMaxElevations.Evaluate(distanceFromCenter);
                
                vertices[i] = vertex.normalized * distanceFromCenter;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            surfaceData.PlanetMaterial.SetFloat(Min, minMaxElevations.Min);
            surfaceData.PlanetMaterial.SetFloat(Max, minMaxElevations.Max);
            
            _meshFilter.sharedMesh = mesh;
        }
    }
}