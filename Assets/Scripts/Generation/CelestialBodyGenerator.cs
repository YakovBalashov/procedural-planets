using System.Linq;
using System.Runtime.InteropServices;
using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public abstract class CelestialBodyGenerator<T> : MonoBehaviour where T : CelestialBodyData
    {
        [Header("Mesh")] [SerializeField, Range(0, 6)]
        private int subdivisionLevel;

        [Header("Surface")] public T bodyData;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private ComputeBuffer _noiseSettingsBuffer;
        
        private readonly int _minId = Shader.PropertyToID("_Min");
        private readonly int _maxId = Shader.PropertyToID("_Max");
        private readonly int _noiseSettingsCountId = Shader.PropertyToID("_NoiseLayerCount");
        private readonly int _noiseSettingsBufferId = Shader.PropertyToID("_NoiseSettings");
        private readonly int _bodyCenterId = Shader.PropertyToID("_PlanetCenter");
        private readonly int _bodyRadiusId = Shader.PropertyToID("_PlanetRadius");
        
        public void UpdateSurface()
        {
            Initialize();
            GenerateMesh();
            UpdateMaterial();
        }
        
        private void Initialize()
        {
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
            if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();
            if (bodyData && bodyData.SurfaceMaterial)
                _meshRenderer.sharedMaterial = bodyData.SurfaceMaterial; 
        }
        
        private void GenerateMesh()
        {
            var mesh = IcoSphereGenerator.Generate(subdivisionLevel, bodyData.Radius);

            var noiseGenerators = (from noiseSetting in bodyData.CPUNoiseSettings
                where noiseSetting.Enabled
                select new NoiseGenerator(noiseSetting)).ToList();
            
            var minMaxElevations = new MinMax();
            var vertices = mesh.vertices;
            
            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];

                var elevation = noiseGenerators.Sum(noiseGenerator => noiseGenerator.Evaluate(vertex.normalized));

                var distanceFromCenter = bodyData.Radius * (1 + elevation);

                minMaxElevations.Evaluate(distanceFromCenter);
                
                vertices[i] = vertex.normalized * distanceFromCenter;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            bodyData.SurfaceMaterial.SetFloat(_minId, minMaxElevations.Min);
            bodyData.SurfaceMaterial.SetFloat(_maxId, minMaxElevations.Max);
            
            _meshFilter.sharedMesh = mesh;
        }
        
        private void UpdateMaterial()
        {
            var gpuNoiseSettings = bodyData.GPUNoiseSettings
                .Where(setting => setting.Enabled)
                .Select(setting => setting.ToGPU())
                .ToArray();
            
            _meshRenderer.sharedMaterial.SetInt(_noiseSettingsCountId, gpuNoiseSettings.Length);
            _meshRenderer.sharedMaterial.SetVector(_bodyCenterId, transform.position);
            _meshRenderer.sharedMaterial.SetFloat(_bodyRadiusId, bodyData.Radius);
            
            if (gpuNoiseSettings.Length == 0) return;
            
            _noiseSettingsBuffer?.Release();
            _noiseSettingsBuffer =
                new ComputeBuffer(gpuNoiseSettings.Length, Marshal.SizeOf(typeof(NoiseSettingsGPU)));
            _noiseSettingsBuffer.SetData(gpuNoiseSettings);
            _meshRenderer.sharedMaterial.SetBuffer(_noiseSettingsBufferId, _noiseSettingsBuffer);
        }
    }
}
