using System.Linq;
using System.Runtime.InteropServices;
using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), ExecuteAlways]
    public abstract class CelestialBodyGenerator<TData, TType> : CelestialBodyGeneratorBase
        where TData : CelestialBodyData
        where TType : CelestialBodyType<TData>
    {
        private static readonly int BaseVertices = Shader.PropertyToID("BaseVertices");
        private static readonly int DisplacedVertices = Shader.PropertyToID("DisplacedVertices");
        private static readonly int PlanetRadius = Shader.PropertyToID("PlanetRadius");
        private static readonly int Normals = Shader.PropertyToID("Normals");
        private static readonly int NormalSampleDistance = Shader.PropertyToID("NormalSampleDistance");

        [Header("Mesh")] [SerializeField, Range(0, 6)]
        private int subdivisionLevel;

        [field: SerializeField] public TData BodyData { get; private set; }
        [field: SerializeField] public TType BodyType { get; private set; }

        [SerializeField] private ComputeShader displacementShader;
        [SerializeField] private float normalSampleDistance = 0.01f;

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
            UpdateTessellationMaterial();
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
            if (!displacementShader)
            {
                Debug.LogFormat(LogType.Warning, LogOption.None, this,
                    "No displacement shader assigned. Cannot generate planet mesh.");
                return;
            }

            var mesh = IcoSphereGenerator.Generate(subdivisionLevel, BodyData.Radius);

            var baseVertices = mesh.vertices;
            var displacedVertices = new Vector3[baseVertices.Length];
            var normals = new Vector3[baseVertices.Length];

            int vec3Size = sizeof(float) * 3;
            ComputeBuffer baseVertexBuffer = new ComputeBuffer(baseVertices.Length, vec3Size);
            ComputeBuffer displacedVertexBuffer = new ComputeBuffer(baseVertices.Length, vec3Size);
            ComputeBuffer normalBuffer = new ComputeBuffer(baseVertices.Length, vec3Size);

            baseVertexBuffer.SetData(baseVertices);
            var gpuNoiseSettings = BodyData.GPUNoiseSettings
                .Where(setting => setting.Enabled)
                .Select(setting => setting.ToGPU())
                .ToArray();

            ComputeBuffer noiseBuffer = new ComputeBuffer(Mathf.Max(1, gpuNoiseSettings.Length),
                Marshal.SizeOf(typeof(NoiseSettingsGPUStruct)));
            if (gpuNoiseSettings.Length > 0) noiseBuffer.SetData(gpuNoiseSettings);

            int kernel = displacementShader.FindKernel("CSMain");
            displacementShader.SetBuffer(kernel, BaseVertices, baseVertexBuffer);
            displacementShader.SetBuffer(kernel, DisplacedVertices, displacedVertexBuffer);
            displacementShader.SetBuffer(kernel, Normals, normalBuffer);
            displacementShader.SetBuffer(kernel, _noiseSettingsBufferId, noiseBuffer);

            displacementShader.SetInt(_noiseSettingsCountId, gpuNoiseSettings.Length);
            displacementShader.SetFloat(PlanetRadius, BodyData.Radius);
            displacementShader.SetFloat(NormalSampleDistance, normalSampleDistance);
            
            int threadGroups = Mathf.CeilToInt(baseVertices.Length / 64f);
            displacementShader.Dispatch(kernel, threadGroups, 1, 1);

            displacedVertexBuffer.GetData(displacedVertices);
            normalBuffer.GetData(normals);

            baseVertexBuffer.Release();
            displacedVertexBuffer.Release();
            noiseBuffer.Release();
            normalBuffer.Release();

            mesh.vertices = displacedVertices;
            mesh.normals = normals;
            mesh.RecalculateBounds();

            _meshFilter.sharedMesh = mesh;
        }

        private void UpdateTessellationMaterial()
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
                new ComputeBuffer(gpuNoiseSettings.Length, Marshal.SizeOf(typeof(NoiseSettingsGPUStruct)));
            _noiseSettingsBuffer.SetData(gpuNoiseSettings);
            _meshRenderer.sharedMaterial.SetBuffer(_noiseSettingsBufferId, _noiseSettingsBuffer);
        }
    }
}