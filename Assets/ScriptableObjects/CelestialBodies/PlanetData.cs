using System.Collections.Generic;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.CelestialBodies
{
    [CreateAssetMenu(fileName = "PlanetSurfaceData", menuName = "Planetary Systems/Celestial Body Data/Planet Data")]
    public class PlanetData : CelestialBodyData
    {
        [field: SerializeField] public CraterGenerationSettings CraterGenerationSettings { get; private set; }
        [field: SerializeField] public Color BaseColor { get; private set; }
        [field: SerializeField] public List<BiomeParameters> Biomes { get; private set; }
        [field: SerializeField] public Texture2D NormalMap { get; private set; }
        [field: SerializeField] public float NormalMapTile { get; private set; }
        [field: SerializeField] public float NormalMapBlend { get; private set; }
        [field: SerializeField] public RingParameters RingParameters { get; private set; }
        [field: SerializeField] public AtmosphereParameters AtmosphereParameters { get; private set; }

        public void InitializePlanet(List<BiomeParameters> biomes, CraterGenerationSettings craterGenerationSettings,
            Color baseColor, Texture2D normalMap, float normalMapTile, float normalMapBlend)
        {
            Biomes = biomes;
            CraterGenerationSettings = craterGenerationSettings;
            BaseColor = baseColor;
            NormalMap = normalMap;
            NormalMapTile = normalMapTile;
            NormalMapBlend = normalMapBlend;
            RingParameters = new RingParameters();
            AtmosphereParameters = new AtmosphereParameters();
        }
    }
}