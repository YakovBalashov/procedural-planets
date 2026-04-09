using UnityEngine;

namespace ProceduralPlanets.Generation
{
    enum BiomeType
    {
        Default = 0,
    }
    
    [System.Serializable]
    public class BiomeParameters
    {
        [SerializeField] private BiomeType biomeType;
        [SerializeField] private Color color;
        [SerializeField] private int noiseType;
        [SerializeField] private float frequency;
        [SerializeField] private int octaves;
        [SerializeField] private float maskThreshold;
        [SerializeField] private float blendFactor;

        public void Initialize(Color newColor, int newNoiseType, float newFrequency, int newOctaves,
            float newMaskThreshold)
        {
            color = newColor;
            noiseType = newNoiseType;
            frequency = newFrequency;
            octaves = newOctaves;
            maskThreshold = newMaskThreshold;
        }

        public BiomeParametersStruct ToStruct()
        {
            return new BiomeParametersStruct
            {
                BiomeType = (int)biomeType,
                Color = new Vector3(color.r, color.g, color.b),
                NoiseType = noiseType,
                Frequency = frequency,
                Octaves = octaves,
                MaskThreshold = maskThreshold,
                BlendFactor = blendFactor,
            };
        }
    }

    public struct BiomeParametersStruct
    {
        public int BiomeType;
        public Vector3 Color;
        public int NoiseType;
        public float Frequency;
        public int Octaves;
        public float MaskThreshold;
        public float BlendFactor;
    }
}