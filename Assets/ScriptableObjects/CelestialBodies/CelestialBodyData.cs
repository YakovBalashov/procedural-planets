using System.Collections.Generic;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.CelestialBodies
{
    public abstract class CelestialBodyData : ScriptableObject
    {
        [field: SerializeField] public float Radius { get; protected set; } = 10f;
        [field: SerializeField] public List<NoiseSettingsGPU> GPUNoiseSettings { get; protected set; } = new();
        [field: SerializeField] public Color BaseColor { get; private set; }
        [field: SerializeField] public List<BiomeParameters> Biomes { get; private set; }
        [field: SerializeField] public Texture2D NormalMap { get; private set; }
        [field: SerializeField] public float NormalMapTile { get; private set; }
        [field: SerializeField] public float NormalMapBlend { get; private set; }
        [field: SerializeField] public float PlayerOrbitRadius { get; private set; }

        
        public void Initialize(float radius, List<NoiseSettingsGPU> gpuNoiseSettings,
            List<BiomeParameters> biomes, Color baseColor, Texture2D normalMap, float normalMapTile, float normalMapBlend, float playerOrbitRatio)
        {
            Radius = radius;
            PlayerOrbitRadius = radius * playerOrbitRatio;
            GPUNoiseSettings = gpuNoiseSettings;
            Biomes = biomes;
            BaseColor = baseColor;
            NormalMap = normalMap;
            NormalMapTile = normalMapTile;
            NormalMapBlend = normalMapBlend;
        }
    }
}
