using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Movement;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "PlanetType", menuName = "Planetary Systems/Celestial Body Types/Planet Type")]
    public class PlanetType : CelestialBodyType<PlanetData>
    {
        [field: SerializeField] public OrbitType<StarType, StarData> StarOrbitType { get; private set; }
        [field: SerializeField] public OrbitType<PlanetType, PlanetData> PlanetOrbitType { get; private set; }
        [field: SerializeField] public List<BiomeType> BiomeTypes { get; private set; }
        [field: SerializeField] public CraterGenerationSettings CraterGenerationSettings { get; private set; }
        [field: SerializeField] public List<Color> PossibleBaseColors { get; private set; }
        [field: SerializeField] public List<Texture2D> PossibleNormalMaps { get; private set; }
        [field: SerializeField] public Vector2 NormalMapTileRange { get; private set; }
        [field: SerializeField] public Vector2 NormalMapBlendRange { get; private set; }

        public override PlanetData CreateInstance(int seed)
        {
            Random.InitState(seed);
            PlanetData instance = base.CreateInstance(seed);

            var biomes = (from biome in BiomeTypes
                where Random.value <= biome.probability
                select biome.GenerateBiomeParameters()).ToList();

            var baseColor = PossibleBaseColors[Random.Range(0, PossibleBaseColors.Count)];
            
            var normalMap = PossibleNormalMaps[Random.Range(0, PossibleNormalMaps.Count)];
            var normalMapTile = Random.Range(NormalMapTileRange.x, NormalMapTileRange.y);
            var normalMapBlend = Random.Range(NormalMapBlendRange.x, NormalMapBlendRange.y);

            instance.InitializePlanet(biomes, CraterGenerationSettings, baseColor, normalMap, normalMapTile, normalMapBlend);

            return instance;
        }
    }
}