using ProceduralPlanets.BaseMesh;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public class AtmosphereGenerator : MeshGenerator
    {
        [SerializeField, Range(1, 4)] private int subdivisions;
        private MeshFilter _meshFilter;

        public void UpdateAtmosphere(AtmosphereParameters parameters, float planetRadius)
        {
            GenerateMesh(parameters, planetRadius);
            UpdateMaterial(parameters);
        }

        private void UpdateMaterial(AtmosphereParameters parameters)
        {
            MeshRenderer.GetPropertyBlock(MaterialPropertyBlock);
            
            MaterialPropertyBlock.SetColor(ShaderParametersIDs.AtmosphereColor, parameters.Color);
            MaterialPropertyBlock.SetInt(ShaderParametersIDs.AtmosphereNoiseType, (int)parameters.NoiseParameters.Type);
            MaterialPropertyBlock.SetInt(ShaderParametersIDs.AtmosphereNoiseOctaves, parameters.NoiseParameters.Octaves);
            MaterialPropertyBlock.SetFloat(ShaderParametersIDs.AtmosphereNoiseFrequency, parameters.NoiseParameters.Frequency);
            MaterialPropertyBlock.SetInt(ShaderParametersIDs.AtmosphereNoiseWarpType, (int)parameters.NoiseParameters.WarpType);
            MaterialPropertyBlock.SetFloat(ShaderParametersIDs.AtmosphereNoiseWarpAmplitude, parameters.NoiseParameters.WarpAmplitude);
            MaterialPropertyBlock.SetVector(ShaderParametersIDs.AtmosphereNoiseRange, parameters.NoiseRange);
            MaterialPropertyBlock.SetFloat(ShaderParametersIDs.AtmosphereMinAlpha, parameters.MinAlpha);
            
            MeshRenderer.SetPropertyBlock(MaterialPropertyBlock);
        }

        private void GenerateMesh(AtmosphereParameters parameters, float planetRadius)
        {
            InitializePropBlock();
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();

            _meshFilter.sharedMesh =
                IcoSphereGenerator.Generate(subdivisions, parameters.RadiusMultiplier * planetRadius);
        }
    }
}