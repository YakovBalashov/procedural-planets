using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetaryRingsGenerator : MeshGenerator
    {
        private MeshFilter _meshFilter;

        public void UpdateRings(RingParameters parameters)
        {
            InitializePropBlock();
            
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
            _meshFilter.sharedMesh = BaseMesh.RingGenerator.Generate(parameters.SegmentCount, parameters.InnerRadius,
                parameters.OuterRadius);

            UpdateMaterial(parameters);
        }

        private void UpdateMaterial(RingParameters parameters)
        {
            MeshRenderer.GetPropertyBlock(MaterialPropertyBlock);
            
            MaterialPropertyBlock.SetColor(ShaderParametersIDs.RingColor, parameters.RingColor);
            MaterialPropertyBlock.SetInt(ShaderParametersIDs.RingNoiseType, (int)parameters.NoiseParameters.Type);
            MaterialPropertyBlock.SetInt(ShaderParametersIDs.RingNoiseOctaves, parameters.NoiseParameters.Octaves);
            MaterialPropertyBlock.SetFloat(ShaderParametersIDs.RingNoiseFrequency, parameters.NoiseParameters.Frequency);
            
            MeshRenderer.SetPropertyBlock(MaterialPropertyBlock);
            
        }
    }
}