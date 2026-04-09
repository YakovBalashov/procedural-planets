using System.Linq;
using System.Runtime.InteropServices;
using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public class PlanetGenerator : CelestialBodyGenerator<PlanetData, PlanetType>
    {
        [SerializeField] private bool useComputeShader = true;
        [SerializeField] private bool useUnityNormals = true;
        [SerializeField] private ComputeShader displacementShader;
        [SerializeField] private float normalSampleDistance = 0.01f;
        [SerializeField] private Shader planetShader;

        private Material _materialInstance;
        private ComputeBuffer _biomeBuffer;

        public override void UpdateSurface()
        {
            base.UpdateSurface();
            UpdateMaterial();
        }

        private void UpdateMaterial()
        {
            if (!_materialInstance)
            {
                _materialInstance = new Material(planetShader);
                _meshRenderer.sharedMaterial = _materialInstance;
            }

            _materialInstance.SetVector(ShaderParametersIDs.BaseColor, BodyData.BaseColor);
            _materialInstance.SetInt(ShaderParametersIDs.BiomeCount, BodyData.Biomes.Count);
            _materialInstance.SetFloat(ShaderParametersIDs.BodyRadius, BodyData.Radius);

            _biomeBuffer?.Release();

            int biomeStructSize = Marshal.SizeOf<BiomeParametersStruct>();
            _biomeBuffer = new ComputeBuffer(Mathf.Max(1, BodyData.Biomes.Count), biomeStructSize);

            if (BodyData.Biomes.Count > 0)
            {
                var biomeStructs = BodyData.Biomes.Select(b => b.ToStruct()).ToArray();
                _biomeBuffer.SetData(biomeStructs);
            }

            if (BodyData.NormalMap)
            {
                _meshRenderer.sharedMaterial.SetTexture(ShaderParametersIDs.NormalMap, BodyData.NormalMap);
            }

            _materialInstance.SetBuffer(ShaderParametersIDs.BiomeParameters, _biomeBuffer);
        }

        protected override void GenerateMesh()
        {
            var mesh = IcoSphereGenerator.Generate(subdivisionLevel, BodyData.Radius);

            if (useComputeShader && displacementShader is not null) mesh = GenerateMeshOnGPU(mesh);
            else mesh = GenerateMeshOnCPU(mesh);

            mesh.RecalculateBounds();

            MeshFilter.sharedMesh = mesh;
        }

        private Mesh GenerateMeshOnCPU(Mesh mesh)
        {
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
            return mesh;
        }

        private Mesh GenerateMeshOnGPU(Mesh mesh)
        {
            var baseVertices = mesh.vertices;

            int vec3Size = sizeof(float) * 3;

            var gpuNoiseSettings = BodyData.GPUNoiseSettings
                .Where(setting => setting.Enabled)
                .Select(setting => setting.ToStruct())
                .ToArray();
            var craters = CraterGenerator.GenerateCraters(BodyData.CraterGenerationSettings, 0);

            using var baseVertexBuffer = new ComputeBuffer(baseVertices.Length, vec3Size);
            using var displacedVertexBuffer = new ComputeBuffer(baseVertices.Length, vec3Size);
            using var normalBuffer = new ComputeBuffer(baseVertices.Length, vec3Size);
            using var noiseBuffer = new ComputeBuffer(Mathf.Max(1, gpuNoiseSettings.Length),
                Marshal.SizeOf(typeof(NoiseSettingsGPUStruct)));
            using var craterBuffer =
                new ComputeBuffer(Mathf.Max(1, craters.Count), Marshal.SizeOf(typeof(CraterParameters)));

            baseVertexBuffer.SetData(baseVertices);
            if (craters.Count > 0) craterBuffer.SetData(craters);
            if (gpuNoiseSettings.Length > 0) noiseBuffer.SetData(gpuNoiseSettings);

            int kernel = displacementShader.FindKernel("CSMain");
            displacementShader.SetBuffer(kernel, ShaderParametersIDs.BaseVertices, baseVertexBuffer);
            displacementShader.SetBuffer(kernel, ShaderParametersIDs.DisplacedVertices, displacedVertexBuffer);
            displacementShader.SetBuffer(kernel, ShaderParametersIDs.Normals, normalBuffer);
            displacementShader.SetBuffer(kernel, ShaderParametersIDs.NoiseSettingsBuffer, noiseBuffer);
            displacementShader.SetBuffer(kernel, ShaderParametersIDs.CraterParameters, craterBuffer);

            displacementShader.SetInt(ShaderParametersIDs.NoiseSettingsCount, gpuNoiseSettings.Length);
            displacementShader.SetFloat(ShaderParametersIDs.BodyRadius, BodyData.Radius);
            displacementShader.SetFloat(ShaderParametersIDs.NormalSampleDistance, normalSampleDistance);
            displacementShader.SetInt(ShaderParametersIDs.CraterCount, craters.Count);

            int threadGroups = Mathf.CeilToInt(baseVertices.Length / 64f);
            displacementShader.Dispatch(kernel, threadGroups, 1, 1);

            var displacedVertices = new Vector3[baseVertices.Length];
            var normals = new Vector3[baseVertices.Length];

            displacedVertexBuffer.GetData(displacedVertices);
            normalBuffer.GetData(normals);

            mesh.vertices = displacedVertices;

            if (useUnityNormals) mesh.RecalculateNormals();
            else mesh.normals = normals;

            return mesh;
        }

        private void OnDestroy()
        {
            _biomeBuffer?.Release();

            if (!_materialInstance) return;
            
            if (Application.isPlaying) Destroy(_materialInstance);
            else DestroyImmediate(_materialInstance);
        }
    }
}