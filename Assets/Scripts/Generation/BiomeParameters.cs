using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [System.Serializable]
    public class BiomeParameters
    {
        [SerializeField] private Color color;
        [SerializeField] private FnlParameters mainNoise;
        [SerializeField] private float maskThreshold;
        [SerializeField] private float blendFactor;
        [SerializeField] private Vector2 heightRange;
        
        public BiomeParameters(Color color, FnlParameters mainNoise, float maskThreshold, float blendFactor)
        {
            this.color = color;
            this.mainNoise = mainNoise;
            this.maskThreshold = maskThreshold;
            this.blendFactor = blendFactor;
        }

        public BiomeParametersStruct ToStruct()
        {
            return new BiomeParametersStruct
            {
                Color = new Vector3(color.r, color.g, color.b),
                MaskThreshold = maskThreshold,
                NoiseType = (int)mainNoise.Type,
                Frequency = mainNoise.Frequency,
                Octaves = mainNoise.Octaves,
                BlendFactor = blendFactor,
                HeightRange = heightRange,
            };
        }
    }

    public struct BiomeParametersStruct
    {
        public Vector3 Color;
        public int NoiseType;
        public float Frequency;
        public int Octaves;
        public float MaskThreshold;
        public float BlendFactor;
        public Vector2 HeightRange;
    }
}