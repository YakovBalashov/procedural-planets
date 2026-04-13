using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetaryRingsGenerator : MonoBehaviour
    {
        [SerializeField] private Shader ringShader;
        private MeshRenderer _meshRenderer;
        private MeshFilter _meshFilter;
        private Material _materialInstance;

        public void UpdateRings(RingParameters parameters)
        {
            if (!_meshFilter) _meshFilter = GetComponent<MeshFilter>();
            _meshFilter.sharedMesh = BaseMesh.RingGenerator.Generate(parameters.SegmentCount, parameters.InnerRadius,
                parameters.OuterRadius);

            UpdateMaterial(parameters);
        }

        private void UpdateMaterial(RingParameters parameters)
        {
            if (!_meshRenderer) _meshRenderer = GetComponent<MeshRenderer>();

            if (!_materialInstance)
            {
                _materialInstance = new Material(ringShader);
                _meshRenderer.sharedMaterial = _materialInstance;
            }

            _materialInstance.SetColor(ShaderParametersIDs.RingColor, parameters.RingColor);
            _materialInstance.SetInt(ShaderParametersIDs.RingNoiseType, (int)parameters.NoiseParameters.Type);
            _materialInstance.SetInt(ShaderParametersIDs.RingNoiseOctaves, parameters.NoiseParameters.Octaves);
            _materialInstance.SetFloat(ShaderParametersIDs.RingNoiseFrequency, parameters.NoiseParameters.Frequency);
        }
    }
}