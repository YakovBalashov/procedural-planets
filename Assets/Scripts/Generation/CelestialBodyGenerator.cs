using System.Linq;
using System.Runtime.InteropServices;
using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public abstract class CelestialBodyGenerator<TData, TType> : CelestialBodyGeneratorBase 
        where TData : CelestialBodyData
        where TType : CelestialBodyType<TData>
    {
        [Header("Mesh")] [SerializeField, Range(0, 6)]
        private int subdivisionLevel;

        [field: SerializeField] public TData BodyData { get; private set; }
        [field: SerializeField] public TType BodyType { get; private set; }

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private ComputeBuffer _noiseSettingsBuffer;

        private readonly int _noiseSettingsCountId = Shader.PropertyToID("_NoiseLayerCount");
        private readonly int _noiseSettingsBufferId = Shader.PropertyToID("_NoiseSettings");
        private readonly int _bodyCenterId = Shader.PropertyToID("_PlanetCenter");
        private readonly int _bodyRadiusId = Shader.PropertyToID("_PlanetRadius");

        public override void GenerateBodyData()
        {
            BodyData = BodyType.CreateInstance(Random.Range(int.MinValue, int.MaxValue));
            UpdateSurface();
        }
        
        public void GenerateBodyData(int seed)
        {
            BodyData = BodyType.CreateInstance(seed);
            UpdateSurface();
        }
        
        public override void UpdateSurface()
        {
            Initialize();
            GenerateMesh();
            UpdateMaterial();
        }

        public override CelestialBodyData GetBodyData()
        {
            return BodyData;
        }
        
        public void SetBodyType(TType newBodyType) 
        {
            BodyType = newBodyType;
            GenerateBodyData();
        }

        protected virtual void Initialize()
        {
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
            if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();
            if (BodyData && BodyData.SurfaceMaterial)
                _meshRenderer.sharedMaterial = BodyData.SurfaceMaterial;
        }

        private void GenerateMesh()
        {
            var mesh = IcoSphereGenerator.Generate(subdivisionLevel, BodyData.Radius);

            var noiseGenerators = (from noiseSetting in BodyData.CPUNoiseSettings
                where noiseSetting.Enabled
                select new NoiseGenerator(noiseSetting)).ToList();

            var minMaxElevations = new MinMax();
            var vertices = mesh.vertices;

            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];

                var elevation = noiseGenerators.Sum(noiseGenerator => noiseGenerator.Evaluate(vertex.normalized));

                var distanceFromCenter = BodyData.Radius * (1 + elevation);

                minMaxElevations.Evaluate(distanceFromCenter);

                vertices[i] = vertex.normalized * distanceFromCenter;
            }

            mesh.vertices = vertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            _meshFilter.sharedMesh = mesh;
        }

        private void UpdateMaterial()
        {
            var gpuNoiseSettings = BodyData.GPUNoiseSettings
                .Where(setting => setting.Enabled)
                .Select(setting => setting.ToGPU())
                .ToArray();

            _meshRenderer.sharedMaterial.SetInt(_noiseSettingsCountId, gpuNoiseSettings.Length);
            _meshRenderer.sharedMaterial.SetVector(_bodyCenterId, transform.position);
            _meshRenderer.sharedMaterial.SetFloat(_bodyRadiusId, BodyData.Radius);

            if (gpuNoiseSettings.Length == 0) return;

            _noiseSettingsBuffer?.Release();
            _noiseSettingsBuffer =
                new ComputeBuffer(gpuNoiseSettings.Length, Marshal.SizeOf(typeof(NoiseSettingsGPU)));
            _noiseSettingsBuffer.SetData(gpuNoiseSettings);
            _meshRenderer.sharedMaterial.SetBuffer(_noiseSettingsBufferId, _noiseSettingsBuffer);
        }
    }
}