using System.Linq;
using System.Runtime.InteropServices;
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
        private static readonly int NoiseSettingsCount = Shader.PropertyToID("_NoiseLayerCount");
        private static readonly int NoiseSettingsBuffer = Shader.PropertyToID("_NoiseSettings");
        private static readonly int PlanetCenter = Shader.PropertyToID("_PlanetCenter");
        private static readonly int PlanetRadius = Shader.PropertyToID("_PlanetRadius");

        [Header("Mesh")] [SerializeField, Range(0, 6)]
        private int subdivisionLevel;

        [Header("Surface")] public PlanetSurfaceData surfaceData;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private ComputeBuffer _noiseSettingsBuffer;
        
        private void OnValidate()
        {
            UpdateSurface();
        }

        public void UpdateSurface()
        {
            Initialize();
            GenerateMesh();
            UpdateMaterial();
        }

        private void UpdateMaterial()
        {
            var gpuNoiseSettings = surfaceData.GPUNoiseSettings
                .Where(setting => setting.Enabled)
                .Select(setting => setting.ToGPU())
                .ToArray();
            
            _meshRenderer.sharedMaterial.SetInt(NoiseSettingsCount, gpuNoiseSettings.Length);
            _meshRenderer.sharedMaterial.SetVector(PlanetCenter, transform.position);
            _meshRenderer.sharedMaterial.SetFloat(PlanetRadius, surfaceData.Radius);
            
            if (gpuNoiseSettings.Length == 0) return;
            
            _noiseSettingsBuffer?.Release();
            _noiseSettingsBuffer =
                new ComputeBuffer(gpuNoiseSettings.Length, Marshal.SizeOf(typeof(NoiseSettingsGPU)));
            _noiseSettingsBuffer.SetData(gpuNoiseSettings);
            _meshRenderer.sharedMaterial.SetBuffer(NoiseSettingsBuffer, _noiseSettingsBuffer);
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

            var noiseGenerators = (from noiseSetting in surfaceData.CPUNoiseSettings
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