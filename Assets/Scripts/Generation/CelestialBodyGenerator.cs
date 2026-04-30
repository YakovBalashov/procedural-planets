using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ProceduralPlanets.BaseMesh;
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
        [SerializeField] private Shader planetShader;
        
        protected MeshFilter MeshFilter;
        private MeshRenderer _meshRenderer;
        private ComputeBuffer _noiseSettingsBuffer;
        private Material _materialInstance;
        private ComputeBuffer _biomeBuffer;

        
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
        
        public void SetBodyData(TData newBodyData)
        {
            BodyData = newBodyData;
            UpdateSurface();
        }
        
        private void UpdateMaterial()
        {
            UpdateVertexRange();

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
                _materialInstance.SetTexture(ShaderParametersIDs.NormalMap, BodyData.NormalMap);
                _materialInstance.SetFloat(ShaderParametersIDs.NormalMapTile, BodyData.NormalMapTile);
                _materialInstance.SetFloat(ShaderParametersIDs.NormalMapBlend, BodyData.NormalMapBlend);
            }

            _materialInstance.SetBuffer(ShaderParametersIDs.BiomeParameters, _biomeBuffer);
        }
        
        private void UpdateVertexRange()
        {
            var vertices = new List<Vector3>();
            MeshFilter.sharedMesh.GetVertices(vertices);

            if (vertices.Count == 0) return;

            var minSquare = float.MaxValue;
            var maxSquare = float.MinValue;

            foreach (var squareMagnitude in vertices.Select(vertex => vertex.sqrMagnitude))
            {
                if (squareMagnitude < minSquare) minSquare = squareMagnitude;
                if (squareMagnitude > maxSquare) maxSquare = squareMagnitude;
            }

            _materialInstance.SetFloat(ShaderParametersIDs.LowestVertexHeight, Mathf.Sqrt(minSquare));
            _materialInstance.SetFloat(ShaderParametersIDs.HighestVertexHeight, Mathf.Sqrt(maxSquare));
        }
        
        protected virtual void Initialize()
        {
            if (!MeshFilter) MeshFilter = GetComponent<MeshFilter>();
            if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();
            
            if (_materialInstance) return;
            _materialInstance = new Material(planetShader);
            _meshRenderer.sharedMaterial = _materialInstance;
        }
        
        private void OnDestroy()
        {
            _biomeBuffer?.Release();

            if (!_materialInstance) return;

            if (Application.isPlaying) Destroy(_materialInstance);
            else DestroyImmediate(_materialInstance);
        }

        protected abstract void GenerateMesh();
    }
}