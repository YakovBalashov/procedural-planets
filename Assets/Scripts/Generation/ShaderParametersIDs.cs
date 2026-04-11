using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public static class ShaderParametersIDs
    {
        public static readonly int BaseVertices = Shader.PropertyToID("_BaseVertices");
        public static readonly int DisplacedVertices = Shader.PropertyToID("_DisplacedVertices");

        public static readonly int Normals = Shader.PropertyToID("_Normals");
        public static readonly int NormalSampleDistance = Shader.PropertyToID("_NormalSampleDistance");

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
    }
}
