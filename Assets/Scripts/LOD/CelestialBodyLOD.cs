using System;
using System.Collections.Generic;
using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.Generation;
using UnityEngine;

namespace ProceduralPlanets.LOD
{
    [RequireComponent(typeof(CelestialBodyGeneratorBase))]
    public class CelestialBodyLOD : MonoBehaviour
    {
        [SerializeField] private List<LODLevel> lodLevels;
        [SerializeField] private int lowestLODResolution = 16;
        
        private static UnityEngine.Camera _mainCamera;

        private List<Mesh> _meshes;
        private MeshFilter _meshFilter;
        private int _currentLODIndex = -1;
        private float _planetRadius;

        private Mesh GenerateLowestLODMesh(Color color)
        {
            var mesh = CubeSphereGenerator.Generate(lowestLODResolution, _planetRadius);
            var colors = new Color[mesh.vertexCount];
            Array.Fill(colors, color);
            mesh.colors = colors;
            return mesh;
        }

        private void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();

            var generator = GetComponent<CelestialBodyGeneratorBase>();
            var data = generator.GetBodyData();
            _planetRadius = data.Radius;

            _meshes = new List<Mesh>(lodLevels.Count);
            foreach (var lodLevel in lodLevels)
            {
                _meshes.Add(generator.GenerateMeshOnGPU(lodLevel.Resolution, generator.Seed));
            }
            
            var color = new Color(data.BaseColor.r, data.BaseColor.g, data.BaseColor.b, data.LowestLodEmissionIntensity);
            _meshes.Add(GenerateLowestLODMesh(color));
            if (!_mainCamera) _mainCamera = UnityEngine.Camera.main;
            SetLOD();
        }

        private void Update()
        {
            if (!_mainCamera) return;
            SetLOD();
        }

        private void SetLOD()
        {
            var distanceToCamera = Vector3.Distance(transform.position, _mainCamera.transform.position);

            if (distanceToCamera <= _planetRadius)
            {
                UpdateMesh(0);
                return;
            }

            var halfFovRadians = _mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            var frustumHeightAtDistance = 2.0f * distanceToCamera * Mathf.Tan(halfFovRadians);
            var screenCoverage = (_planetRadius * 2f) / frustumHeightAtDistance;

            var newLODIndex = lodLevels.Count;
            for (var i = 0; i < lodLevels.Count; i++)
            {
                if (screenCoverage < lodLevels[i].ScreenCoverageThreshold) continue;
                newLODIndex = i;
                break;
            }

            UpdateMesh(newLODIndex);
        }

        private void UpdateMesh(int index)
        {
            if (index == _currentLODIndex) return;

            _currentLODIndex = index;
            _meshFilter.mesh = _meshes[_currentLODIndex];
        }
    }
}