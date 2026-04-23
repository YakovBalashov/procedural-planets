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
        [Header("Mesh")] [SerializeField, Range(0, 6)]
        protected int subdivisionLevel;

        [field: SerializeField] public TData BodyData { get; private set; }
        [field: SerializeField] public TType BodyType { get; private set; }
        
        protected MeshFilter MeshFilter;
        protected MeshRenderer _meshRenderer;
        private ComputeBuffer _noiseSettingsBuffer;
        
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

        protected virtual void Initialize()
        {
            if (!MeshFilter) MeshFilter = GetComponent<MeshFilter>();
            if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();
            if (BodyData && BodyData.SurfaceMaterial)
                _meshRenderer.sharedMaterial = BodyData.SurfaceMaterial;
        }

        protected abstract void GenerateMesh();
    }
}