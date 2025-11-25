using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.Noise;
using ProceduralPlanets.Surface;
using UnityEngine;

namespace ProceduralPlanets
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetSurface : MonoBehaviour
    {
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
        }

        private void GenerateMesh()
        {
            var mesh = BaseMesh.IcoSphereGenerator.Generate(subdivisionLevel, surfaceData.radius);

            var noiseGenerators = (from noiseSetting in surfaceData.noiseSettings
                where noiseSetting.Enabled
                select new NoiseGenerator(noiseSetting)).ToList();

            var vertices = mesh.vertices;
            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = mesh.vertices[i];

                var elevation = noiseGenerators.Sum(noiseGenerator => noiseGenerator.Evaluate(vertex.normalized));

                vertices[i] = vertex.normalized * (surfaceData.radius * (1 + elevation));
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            _meshFilter.sharedMesh = mesh;
        }
    }
}