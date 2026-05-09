using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public static class ShaderParametersIDs
    {
        public static readonly int BaseVertices = Shader.PropertyToID("_BaseVertices");
        public static readonly int DisplacedVertices = Shader.PropertyToID("_DisplacedVertices");

        public static readonly int Normals = Shader.PropertyToID("_Normals");
        public static readonly int NormalSampleDistance = Shader.PropertyToID("_NormalSampleDistance");
        
        public static readonly int VertexColors = Shader.PropertyToID("_VertexColors");
        
        public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        public static readonly int NormalMap = Shader.PropertyToID("_NormalMap");
        public static readonly int NormalMapTile = Shader.PropertyToID("_Tile");
        public static readonly int NormalMapBlend = Shader.PropertyToID("_Blend");

        public static readonly int CraterParameters = Shader.PropertyToID("_Craters");
        public static readonly int CraterCount = Shader.PropertyToID("_CraterCount");

        public static readonly int BiomeParameters = Shader.PropertyToID("_Biomes");
        public static readonly int BiomeCount = Shader.PropertyToID("_BiomeCount");

        public static readonly int NoiseSettingsCount = Shader.PropertyToID("_NoiseLayerCount");
        public static readonly int NoiseSettingsBuffer = Shader.PropertyToID("_NoiseSettings");

        public static readonly int BodyCenter = Shader.PropertyToID("_PlanetCenter");
        public static readonly int BodyRadius = Shader.PropertyToID("_PlanetRadius");
        
        public static readonly int LowestVertexHeight = Shader.PropertyToID("_LowestVertexHeight");
        public static readonly int HighestVertexHeight = Shader.PropertyToID("_HighestVertexHeight");
        
        public static readonly int RingColor = Shader.PropertyToID("_RingColor");
        public static readonly int RingNoiseType = Shader.PropertyToID("_RingNoiseType");
        public static readonly int RingNoiseOctaves = Shader.PropertyToID("_RingNoiseOctaves");
        public static readonly int RingNoiseFrequency = Shader.PropertyToID("_RingNoiseFrequency");
        
        public static readonly int AtmosphereColor = Shader.PropertyToID("_AtmosphereColor");
        public static readonly int AtmosphereNoiseType = Shader.PropertyToID("_AtmosphereNoiseType");
        public static readonly int AtmosphereNoiseOctaves = Shader.PropertyToID("_AtmosphereNoiseOctaves");
        public static readonly int AtmosphereNoiseFrequency = Shader.PropertyToID("_AtmosphereNoiseFrequency");
        public static readonly int AtmosphereNoiseWarpType = Shader.PropertyToID("_AtmosphereNoiseWarpType");
        public static readonly int AtmosphereNoiseWarpAmplitude = Shader.PropertyToID("_AtmosphereNoiseWarpAmplitude");
        public static readonly int AtmosphereNoiseRange = Shader.PropertyToID("_AtmosphereNoiseRange");
        public static readonly int AtmosphereMinAlpha = Shader.PropertyToID("_AtmosphereMinAlpha");
        
        public static readonly int RotationAxis = Shader.PropertyToID("_Axis");
        public static readonly int RotationAngle = Shader.PropertyToID("_Rotation");
    }
}
