using ProceduralPlanets.Surface;
using UnityEngine;

namespace ProceduralPlanets
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetSurface : MonoBehaviour
    {
        [Header("Mesh")] [SerializeField, Range(0, 6)]
        private int subdivisionLevel;

        [Header("Surface")] 
        public PlanetSurfaceData surfaceData;

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
            if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();
            if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
        }

        private void GenerateMesh()
        {
            var mesh = BaseMesh.IcoSphereGenerator.Generate(subdivisionLevel, surfaceData.radius);
            _meshFilter.sharedMesh = mesh;
        }
    }
}