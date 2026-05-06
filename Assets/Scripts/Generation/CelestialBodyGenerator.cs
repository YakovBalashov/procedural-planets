using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.Movement;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
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
        [Range(2, 256), SerializeField]
        [Tooltip("Number of vertices along one edge of a single face.")]
        protected int resolution = 10;

        [field: SerializeField] public TData BodyData { get; private set; }
        [field: SerializeField] public TType BodyType { get; private set; }
        [SerializeField] private bool useUnityNormals = true;
        [SerializeField] private ComputeShader displacementShader;
        [SerializeField] private float normalSampleDistance = 0.01f;

        private MeshFilter _meshFilter;
        private ComputeBuffer _noiseSettingsBuffer;
        private ComputeBuffer _biomeBuffer;
        private AxisRotation _axisRotation;


        public override void GenerateBodyData()
        {
            GenerateBodyData(Random.Range(0, int.MaxValue));
        }

        public void GenerateBodyData(int seed)
        {
            BodyData = BodyType.CreateInstance(seed);
            UpdateSurface();
        }

        public override void UpdateSurface()
        {
            Initialize();
            UpdateAxis();
            UpdateMesh();
            UpdateMaterial();
        }

        private void UpdateAxis()
        {
            if (!_axisRotation) _axisRotation = GetComponent<AxisRotation>();
            if (!_axisRotation) return;
            _axisRotation.SetParameters(BodyData.RotationAxis, BodyData.RotationSpeedInDegreesPerSecond);
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

        public void SetBodyData(TData newBodyData)
        {
            BodyData = newBodyData;
            UpdateSurface();
        }

        public override void UpdateMaterial()
        {
            UpdateVertexRange();

            MeshRenderer.GetPropertyBlock(MaterialPropertyBlock);
            
            MaterialPropertyBlock.SetVector(ShaderParametersIDs.BaseColor, BodyData.BaseColor);
            MaterialPropertyBlock.SetInt(ShaderParametersIDs.BiomeCount, BodyData.Biomes.Count);
            MaterialPropertyBlock.SetFloat(ShaderParametersIDs.BodyRadius, BodyData.Radius);

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
                MaterialPropertyBlock.SetTexture(ShaderParametersIDs.NormalMap, BodyData.NormalMap);
                MaterialPropertyBlock.SetFloat(ShaderParametersIDs.NormalMapTile, BodyData.NormalMapTile);
                MaterialPropertyBlock.SetFloat(ShaderParametersIDs.NormalMapBlend, BodyData.NormalMapBlend);
            }

            MaterialPropertyBlock.SetBuffer(ShaderParametersIDs.BiomeParameters, _biomeBuffer);
            
            MeshRenderer.SetPropertyBlock(MaterialPropertyBlock);
        }

        private void UpdateMesh()
        {
            if (!displacementShader) return;
            
            var mesh = GenerateMeshOnGPU(resolution);
            _meshFilter.sharedMesh = mesh;
        }

        protected virtual ComputeBuffer CreateCraterBuffer()
        {
            return new ComputeBuffer(1, sizeof(byte));
        }

        public override Mesh GenerateMeshOnGPU(int sphereResolution)
        {
            var mesh = CubeSphereGenerator.Generate(sphereResolution, BodyData.Radius);
            
            var baseVertices = mesh.vertices;

            var vector3Size = Marshal.SizeOf(typeof(Vector3));

            var gpuNoiseSettings = BodyData.GPUNoiseSettings
                .Where(setting => setting.Enabled)
                .Select(setting => setting.ToStruct())
                .ToArray();

            using var baseVertexBuffer = new ComputeBuffer(baseVertices.Length, vector3Size);
            using var displacedVertexBuffer = new ComputeBuffer(baseVertices.Length, vector3Size);
            using var normalBuffer = new ComputeBuffer(baseVertices.Length, vector3Size);
            using var noiseBuffer = new ComputeBuffer(Mathf.Max(1, gpuNoiseSettings.Length),
                Marshal.SizeOf(typeof(NoiseSettingsGPUStruct)));
            using var colorBuffer = new ComputeBuffer(baseVertices.Length, Marshal.SizeOf(typeof(Color)));
            using var biomeBuffer = new ComputeBuffer(Mathf.Max(1, BodyData.Biomes.Count),
                Marshal.SizeOf(typeof(BiomeParametersStruct)));
            using var craterBuffer = CreateCraterBuffer();
            var craterCount = (craterBuffer.stride < Marshal.SizeOf(typeof(CraterParameters))) ? 0 : craterBuffer.count;

            baseVertexBuffer.SetData(baseVertices);
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
            
            displacementShader.SetInt(ShaderParametersIDs.CraterCount, craterCount);
            displacementShader.SetInt(ShaderParametersIDs.NoiseSettingsCount, gpuNoiseSettings.Length);
            displacementShader.SetFloat(ShaderParametersIDs.BodyRadius, BodyData.Radius);
            displacementShader.SetFloat(ShaderParametersIDs.NormalSampleDistance, normalSampleDistance);

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
            mesh.RecalculateBounds();

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

        private void UpdateVertexRange()
        {
            var range = GetVertexHeightRange(_meshFilter.sharedMesh);
            
            MeshRenderer.GetPropertyBlock(MaterialPropertyBlock);
            MaterialPropertyBlock.SetFloat(ShaderParametersIDs.LowestVertexHeight, range.x);
            MaterialPropertyBlock.SetFloat(ShaderParametersIDs.HighestVertexHeight, range.y);
            MeshRenderer.SetPropertyBlock(MaterialPropertyBlock);
        }

        private static Vector2 GetVertexHeightRange(Mesh mesh)
        {
            var vertices = new List<Vector3>();
            mesh.GetVertices(vertices);

            if (vertices.Count == 0) return Vector2.zero;

            var minSquare = float.MaxValue;
            var maxSquare = float.MinValue;

            foreach (var squareMagnitude in vertices.Select(vertex => vertex.sqrMagnitude))
            {
                if (squareMagnitude < minSquare) minSquare = squareMagnitude;
                if (squareMagnitude > maxSquare) maxSquare = squareMagnitude;
            }

            return new Vector2(Mathf.Sqrt(minSquare), Mathf.Sqrt(maxSquare));
        }

        protected virtual void Initialize()
        {
            InitializePropBlock();
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
        }

        private void OnDestroy()
        {
            _biomeBuffer?.Release();
        }
    }
}