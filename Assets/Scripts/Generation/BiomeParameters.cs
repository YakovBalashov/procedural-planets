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
    public struct BiomeColors
    {
        public Color baseColor;
        public Color accentColor;
    }

    [System.Serializable]
    public class BiomeParameters
    {
        [SerializeField] private BiomeFeatures features;
        [SerializeField] private Color color;
        [SerializeField] private Color accentColor;
        [SerializeField] private BiomeColors biomeColors;
        [SerializeField] private FnlParameters mainNoise;
        [SerializeField] private float maskThreshold;
        [SerializeField] private float blendFactor;
        [SerializeField] private float accentThreshold;
        [SerializeField] private float accentBlendFactor;
        [SerializeField] private Vector2 heightRange;
        [SerializeField] private Vector2 steepnessRange;
        [SerializeField] private float poleAngle;
        [SerializeField] private Vector3 poleDirection;
        [SerializeField] private Vector3 stripesAxis;
        [SerializeField] private float stripesScale;
        [SerializeField] private float emissionIntensity;

        public BiomeParameters CreateCopy(Vector3 offset, BiomeColors randomColor)
        {
            var randomizedParameters = new BiomeParameters
            {
                features = features,
                biomeColors = randomColor,
                mainNoise = mainNoise,
                maskThreshold = maskThreshold,
                blendFactor = blendFactor,
                accentThreshold = accentThreshold,
                accentBlendFactor = accentBlendFactor,
                heightRange = heightRange,
                steepnessRange = steepnessRange,
                poleAngle = poleAngle,
                poleDirection = poleDirection,
                stripesAxis = stripesAxis,
                stripesScale = stripesScale,
                emissionIntensity = emissionIntensity
            };

            if (features.HasFlag(BiomeFeatures.MainNoise))
            {
                mainNoise.SetOffset(offset);
            }

            return randomizedParameters;
        }

        public BiomeParametersStruct ToStruct()
        {
            return new BiomeParametersStruct
            {
                Features = (uint)features,
                Color = new Vector3(biomeColors.baseColor.r, biomeColors.baseColor.g, biomeColors.baseColor.b),
                AccentColor = new Vector3(biomeColors.accentColor.r, biomeColors.accentColor.g, biomeColors.accentColor.b),
                MaskThreshold = maskThreshold,
                NoiseType = (int)mainNoise.Type,
                WarpType =  (int)mainNoise.WarpType,
                WarpAmplitude = mainNoise.WarpAmplitude,
                Frequency = mainNoise.Frequency,
                Octaves = mainNoise.Octaves,
                Offset = mainNoise.Offset,
                BlendFactor = blendFactor,
                AccentThreshold = accentThreshold,
                AccentBlendFactor = accentBlendFactor,
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
        public Vector3 AccentColor;
        public int NoiseType;
        public int WarpType;
        public float WarpAmplitude;
        public float Frequency;
        public int Octaves;
        public Vector3 Offset;
        public float MaskThreshold;
        public float BlendFactor;
        public float AccentThreshold;
        public float AccentBlendFactor;
        public Vector2 HeightRange;
        public Vector2 SteepnessRange;
        public float PoleAngle;
        public Vector3 PoleDirection;
        public Vector3 StripesAxis;
        public float StripesScale;
        public float EmissionIntensity;
    }
}