using System.Collections.Generic;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.Generation;
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

        public void InitializePlanet(List<BiomeParameters> biomes, CraterGenerationSettings craterGenerationSettings, Color baseColor)
        {
            Biomes = biomes;
            CraterGenerationSettings = craterGenerationSettings;
            BaseColor = baseColor;
        }
    }
}