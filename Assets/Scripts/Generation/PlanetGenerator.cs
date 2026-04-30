using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralPlanets.Generation
{
    public class PlanetGenerator : CelestialBodyGenerator<PlanetData, PlanetType>
    {
        [SerializeField] private bool useComputeShader = true;
        [SerializeField] private bool useUnityNormals = true;
        [SerializeField] private ComputeShader displacementShader;
        [SerializeField] private float normalSampleDistance = 0.01f;

        [SerializeField] private GameObject atmospherePrefab;
        [SerializeField] private GameObject ringsPrefab;

        public List<GameObject> Moons { get; set; }

        private PlanetaryRingsGenerator _ringsGenerator;
        private AtmosphereGenerator _atmosphereGenerator;

        protected override void Initialize()
        {
            base.Initialize();

            if (BodyData.AtmosphereParameters.Enabled && !_atmosphereGenerator)
            {
                _atmosphereGenerator = Instantiator.InstantiateGameObject(atmospherePrefab, transform)
                    .GetComponent<AtmosphereGenerator>();
            }

            if (BodyData.RingParameters.Enabled && !_ringsGenerator)
            {
                _ringsGenerator = Instantiator.InstantiateGameObject(ringsPrefab, transform)
                    .GetComponent<PlanetaryRingsGenerator>();
            }
        }

        public override void UpdateSurface()
        {
            base.UpdateSurface();

            if (BodyData.AtmosphereParameters.Enabled)
                _atmosphereGenerator.UpdateAtmosphere(BodyData.AtmosphereParameters, BodyData.Radius);

            if (BodyData.RingParameters.Enabled) _ringsGenerator.UpdateRings(BodyData.RingParameters);
        }

        protected override void GenerateMesh()
        {
            var mesh = CubeSphereGenerator.Generate(resolution, BodyData.Radius);

            if (useComputeShader && displacementShader is not null) mesh = GenerateMeshOnGPU(mesh);

            mesh.RecalculateBounds();

            MeshFilter.sharedMesh = mesh;
        }

        private Mesh GenerateMeshOnGPU(Mesh mesh)
        {
            var baseVertices = mesh.vertices;

            var vector3Size = Marshal.SizeOf(typeof(Vector3));

            var gpuNoiseSettings = BodyData.GPUNoiseSettings
                .Where(setting => setting.Enabled)
                .Select(setting => setting.ToStruct())
                .ToArray();
            var craters = CraterGenerator.GenerateCraters(BodyData.CraterGenerationSettings, 0);

            using var baseVertexBuffer = new ComputeBuffer(baseVertices.Length, vector3Size);
            using var displacedVertexBuffer = new ComputeBuffer(baseVertices.Length, vector3Size);
            using var normalBuffer = new ComputeBuffer(baseVertices.Length, vector3Size);
            using var noiseBuffer = new ComputeBuffer(Mathf.Max(1, gpuNoiseSettings.Length),
                Marshal.SizeOf(typeof(NoiseSettingsGPUStruct)));
            using var craterBuffer =
                new ComputeBuffer(Mathf.Max(1, craters.Count), Marshal.SizeOf(typeof(CraterParameters)));
            using var colorBuffer = new ComputeBuffer(baseVertices.Length, Marshal.SizeOf(typeof(Color)));
            using var biomeBuffer = new ComputeBuffer(Mathf.Max(1, BodyData.Biomes.Count), Marshal.SizeOf(typeof(BiomeParametersStruct)));

            baseVertexBuffer.SetData(baseVertices);
            if (craters.Count > 0) craterBuffer.SetData(craters);
            if (gpuNoiseSettings.Length > 0) noiseBuffer.SetData(gpuNoiseSettings);
            if (BodyData.Biomes.Count > 0)
            {
                var biomeStructs = BodyData.Biomes.Select(b => b.ToStruct()).ToArray();
                biomeBuffer.SetData(biomeStructs);
            }

            int geometryKernel = displacementShader.FindKernel("CSGeometry");
            displacementShader.SetBuffer(geometryKernel, ShaderParametersIDs.BaseVertices, baseVertexBuffer);
            displacementShader.SetBuffer(geometryKernel, ShaderParametersIDs.DisplacedVertices, displacedVertexBuffer);
            displacementShader.SetBuffer(geometryKernel, ShaderParametersIDs.Normals, normalBuffer);
            displacementShader.SetBuffer(geometryKernel, ShaderParametersIDs.NoiseSettingsBuffer, noiseBuffer);
            displacementShader.SetBuffer(geometryKernel, ShaderParametersIDs.CraterParameters, craterBuffer);
            
            displacementShader.SetInt(ShaderParametersIDs.NoiseSettingsCount, gpuNoiseSettings.Length);
            displacementShader.SetFloat(ShaderParametersIDs.BodyRadius, BodyData.Radius);
            displacementShader.SetFloat(ShaderParametersIDs.NormalSampleDistance, normalSampleDistance);
            displacementShader.SetInt(ShaderParametersIDs.CraterCount, craters.Count);

            int threadGroups = Mathf.CeilToInt(baseVertices.Length / 64f);
            displacementShader.Dispatch(geometryKernel, threadGroups, 1, 1);

            var displacedVertices = new Vector3[baseVertices.Length];
            var normals = new Vector3[baseVertices.Length];
            
            displacedVertexBuffer.GetData(displacedVertices);
            normalBuffer.GetData(normals);

            mesh.vertices = displacedVertices;
            if (useUnityNormals)
            {
                mesh.RecalculateNormals();
                normalBuffer.SetData(mesh.normals);
            }
            else mesh.normals = normals;

            var heightRange = GetVertexHeightRange(mesh);
            
            int colorKernel = displacementShader.FindKernel("CSColor");

            displacementShader.SetBuffer(colorKernel, ShaderParametersIDs.BiomeParameters, biomeBuffer);
            displacementShader.SetBuffer(colorKernel, ShaderParametersIDs.VertexColors, colorBuffer);
            displacementShader.SetBuffer(colorKernel, ShaderParametersIDs.DisplacedVertices, displacedVertexBuffer);
            displacementShader.SetBuffer(colorKernel, ShaderParametersIDs.Normals, normalBuffer);
            
            displacementShader.SetVector(ShaderParametersIDs.BaseColor, BodyData.BaseColor);
            displacementShader.SetInt(ShaderParametersIDs.BiomeCount, BodyData.Biomes.Count);
            displacementShader.SetFloat(ShaderParametersIDs.BodyRadius, BodyData.Radius);
            displacementShader.SetFloat(ShaderParametersIDs.LowestVertexHeight, heightRange.x);
            displacementShader.SetFloat(ShaderParametersIDs.HighestVertexHeight, heightRange.y);
            
            displacementShader.Dispatch(colorKernel, threadGroups, 1, 1);
            
            var colors = new Color[baseVertices.Length];
            colorBuffer.GetData(colors);
            mesh.colors = colors;
            
            return mesh;
        }

#if UNITY_EDITOR
        [ContextMenu("Save Planet Data to Asset")]
        public void SavePlanetDataToAsset()
        {
            if (BodyData == null) return;

            if (EditorUtility.IsPersistent(BodyData))
            {
                EditorUtility.SetDirty(BodyData);
                AssetDatabase.SaveAssets();
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Planet Data",
                $"{gameObject.name}",
                "asset",
                "Save procedural planet data"
            );

            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(BodyData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=cyan>Saved Planet Data:</color> {path}");
        }
#endif
    }
}