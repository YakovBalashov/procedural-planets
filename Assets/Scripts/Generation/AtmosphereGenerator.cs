using ProceduralPlanets.BaseMesh;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class AtmosphereGenerator : MonoBehaviour
    {
        [SerializeField] private Shader atmosphereShader;
        [SerializeField, Range(1, 4)] private int subdivisions;
        private Material _materialInstance;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;

        public void UpdateAtmosphere(AtmosphereParameters parameters, float planetRadius)
        {
            GenerateMesh(parameters, planetRadius);
            UpdateMaterial(parameters);
        }

        private void UpdateMaterial(AtmosphereParameters parameters)
        {
            if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();

            if (!_materialInstance)
            {
                _materialInstance = new Material(atmosphereShader);
                _meshRenderer.sharedMaterial = _materialInstance;
            }

            _materialInstance.SetColor(ShaderParametersIDs.AtmosphereColor, parameters.Color);
            _materialInstance.SetInt(ShaderParametersIDs.AtmosphereNoiseType, (int)parameters.NoiseParameters.Type);
            _materialInstance.SetInt(ShaderParametersIDs.AtmosphereNoiseOctaves, parameters.NoiseParameters.Octaves);
            _materialInstance.SetFloat(ShaderParametersIDs.AtmosphereNoiseFrequency, parameters.NoiseParameters.Frequency);
            _materialInstance.SetInt(ShaderParametersIDs.AtmosphereNoiseWarpType, (int)parameters.NoiseParameters.WarpType);
            _materialInstance.SetFloat(ShaderParametersIDs.AtmosphereNoiseWarpAmplitude, parameters.NoiseParameters.WarpAmplitude);
            _materialInstance.SetVector(ShaderParametersIDs.AtmosphereNoiseRange, parameters.NoiseRange);
            _materialInstance.SetFloat(ShaderParametersIDs.AtmosphereMinAlpha, parameters.MinAlpha);
        }

        private void GenerateMesh(AtmosphereParameters parameters, float planetRadius)
        {
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
            if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();

            _meshFilter.sharedMesh =
                IcoSphereGenerator.Generate(subdivisions, parameters.RadiusMultiplier * planetRadius);
        }
    }
}