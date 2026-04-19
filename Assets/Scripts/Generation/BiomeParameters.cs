using System;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [Flags]
    public enum BiomeFeatures
    {
        None = 0,
        MainNoise = 1 << 0,
        EdgeNoise = 1 << 1,
        HeightRange = 1 << 2,
        SteepnessRange = 1 << 3,
        Poles = 1 << 4,
        Stripes = 1 << 5,
        Emission = 1 << 6,
    }

    [System.Serializable]
    public class BiomeParameters
    {
        [SerializeField] private BiomeFeatures features;
        [SerializeField] private Color color;
        [SerializeField] private FnlParameters mainNoise;
        [SerializeField] private float maskThreshold;
        [SerializeField] private float blendFactor;
        [SerializeField] private Vector2 heightRange;
        [SerializeField] private Vector2 steepnessRange;
        [SerializeField] private float poleAngle;
        [SerializeField] private Vector3 poleDirection;
        [SerializeField] private Vector3 stripesAxis;
        [SerializeField] private float stripesScale;
        [SerializeField] private float emissionIntensity;

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
                Features = (uint)features,
                Color = new Vector3(color.r, color.g, color.b),
                MaskThreshold = maskThreshold,
                NoiseType = (int)mainNoise.Type,
                WarpType =  (int)mainNoise.WarpType,
                WarpAmplitude = mainNoise.WarpAmplitude,
                Frequency = mainNoise.Frequency,
                Octaves = mainNoise.Octaves,
                BlendFactor = blendFactor,
                HeightRange = heightRange,
                SteepnessRange = steepnessRange,
                PoleAngle = poleAngle,
                PoleDirection = poleDirection,
                StripesAxis = stripesAxis,
                StripesScale = stripesScale,
                EmissionIntensity = emissionIntensity
            };
        }
    }

    public struct BiomeParametersStruct
    {
        public uint Features;
        public Vector3 Color;
        public int NoiseType;
        public int WarpType;
        public float WarpAmplitude;
        public float Frequency;
        public int Octaves;
        public float MaskThreshold;
        public float BlendFactor;
        public Vector2 HeightRange;
        public Vector2 SteepnessRange;
        public float PoleAngle;
        public Vector3 PoleDirection;
        public Vector3 StripesAxis;
        public float StripesScale;
        public float EmissionIntensity;
    }
}