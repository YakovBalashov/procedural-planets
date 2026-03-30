using System;
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
        [SerializeField] private Material planetMaterial;
        [SerializeField] private Shader planetShader;

        private static readonly int BaseVertices = Shader.PropertyToID("BaseVertices");
        private static readonly int DisplacedVertices = Shader.PropertyToID("DisplacedVertices");
        private static readonly int PlanetRadius = Shader.PropertyToID("PlanetRadius");
        private static readonly int Normals = Shader.PropertyToID("Normals");
        private static readonly int NormalSampleDistance = Shader.PropertyToID("NormalSampleDistance");
        private static readonly int CraterParameters = Shader.PropertyToID("Craters");
        private static readonly int BiomeParameters = Shader.PropertyToID("Biomes");

        private readonly int _noiseSettingsCountId = Shader.PropertyToID("_NoiseLayerCount");
        private readonly int _noiseSettingsBufferId = Shader.PropertyToID("_NoiseSettings");
        private readonly int _bodyCenterId = Shader.PropertyToID("_PlanetCenter");
        private readonly int _bodyRadiusId = Shader.PropertyToID("_PlanetRadius");
        private readonly int _craterCountId = Shader.PropertyToID("CraterCount");
        private readonly int _biomeCountId = Shader.PropertyToID("BiomeCount");
        private readonly int _baseColorId = Shader.PropertyToID("BaseColor");

        private ComputeBuffer _biomeBuffer;

        private void OnEnable()
        {
            UpdateSurface();
        }

        public override void UpdateSurface()
        {
            base.UpdateSurface();
            UpdateMaterial();
        }

        private void UpdateMaterial()
        {
            _meshRenderer.sharedMaterial = new Material(planetMaterial);
            _meshRenderer.sharedMaterial.SetInt(_biomeCountId, BodyData.Biomes.Count);
            var gpuColor = new Vector4(BodyData.BaseColor.r, BodyData.BaseColor.g, BodyData.BaseColor.b, 1f);
            _meshRenderer.sharedMaterial.SetVector(_baseColorId, gpuColor);
            _meshRenderer.sharedMaterial.SetInt(_biomeCountId, BodyData.Biomes.Count);
            
            _biomeBuffer?.Release();
            int biomeStructSize = Marshal.SizeOf<BiomeParametersStruct>();
            _biomeBuffer = new ComputeBuffer(Mathf.Max(1, BodyData.Biomes.Count), biomeStructSize);
            if (BodyData.Biomes.Count > 0)            {
                var biomeStructs = BodyData.Biomes.Select(b => b.ToStruct()).ToArray();
                _biomeBuffer.SetData(biomeStructs);
            }
            
            _meshRenderer.sharedMaterial.SetBuffer(BiomeParameters, _biomeBuffer);
        }

        private void OnDestroy()
        {
            _biomeBuffer?.Release();
        }

        protected override void GenerateMesh()
        {
            var mesh = IcoSphereGenerator.Generate(subdivisionLevel, BodyData.Radius);

            if (useComputeShader && displacementShader is not null)
            {
                mesh = GenerateMeshOnGPU(mesh);
            }
            else
            {
                mesh = GenerateMeshOnCPU(mesh);
            }

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
            var displacedVertices = new Vector3[baseVertices.Length];
            var normals = new Vector3[baseVertices.Length];

            int vec3Size = sizeof(float) * 3;
            var baseVertexBuffer = new ComputeBuffer(baseVertices.Length, vec3Size);
            var displacedVertexBuffer = new ComputeBuffer(baseVertices.Length, vec3Size);
            var normalBuffer = new ComputeBuffer(baseVertices.Length, vec3Size);

            baseVertexBuffer.SetData(baseVertices);
            var gpuNoiseSettings = BodyData.GPUNoiseSettings
                .Where(setting => setting.Enabled)
                .Select(setting => setting.ToStruct())
                .ToArray();

            ComputeBuffer noiseBuffer = new ComputeBuffer(Mathf.Max(1, gpuNoiseSettings.Length),
                Marshal.SizeOf(typeof(NoiseSettingsGPUStruct)));
            if (gpuNoiseSettings.Length > 0) noiseBuffer.SetData(gpuNoiseSettings);

            var craters = CraterGenerator.GenerateCraters(BodyData.CraterGenerationSettings, 0);
            int craterStructSize = Marshal.SizeOf<CraterParameters>();
            var craterBuffer = new ComputeBuffer(Mathf.Max(1, craters.Count), craterStructSize);
            

            craterBuffer.SetData(craters);

            int kernel = displacementShader.FindKernel("CSMain");
            displacementShader.SetBuffer(kernel, BaseVertices, baseVertexBuffer);
            displacementShader.SetBuffer(kernel, DisplacedVertices, displacedVertexBuffer);
            displacementShader.SetBuffer(kernel, Normals, normalBuffer);
            displacementShader.SetBuffer(kernel, _noiseSettingsBufferId, noiseBuffer);
            displacementShader.SetBuffer(kernel, CraterParameters, craterBuffer);
            
            displacementShader.SetInt(_noiseSettingsCountId, gpuNoiseSettings.Length);
            displacementShader.SetFloat(PlanetRadius, BodyData.Radius);
            displacementShader.SetFloat(NormalSampleDistance, normalSampleDistance);
            displacementShader.SetInt(_craterCountId, craters.Count);

            int threadGroups = Mathf.CeilToInt(baseVertices.Length / 64f);
            displacementShader.Dispatch(kernel, threadGroups, 1, 1);

            displacedVertexBuffer.GetData(displacedVertices);
            normalBuffer.GetData(normals);

            baseVertexBuffer.Release();
            displacedVertexBuffer.Release();
            noiseBuffer.Release();
            normalBuffer.Release();
            craterBuffer.Release();

            mesh.vertices = displacedVertices;
            if (useUnityNormals)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                mesh.normals = normals;
            }

            return mesh;
        }
    }
}