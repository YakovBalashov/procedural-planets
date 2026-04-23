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
        [field: SerializeField] public List<BiomeCandidate> BiomeCandidates { get; private set; }
        [field: SerializeField] public CraterGenerationSettings CraterGenerationSettings { get; private set; }
        [field: SerializeField] public List<Color> PossibleBaseColors { get; private set; }
        [field: SerializeField] public List<Texture2D> PossibleNormalMaps { get; private set; }
        [field: SerializeField] public Vector2 NormalMapTileRange { get; private set; }
        [field: SerializeField] public Vector2 NormalMapBlendRange { get; private set; }

        public override PlanetData CreateInstance(int seed)
        {
            var random = new System.Random(seed);
            PlanetData instance = base.CreateInstance(seed);

            Dictionary<int, List<BiomeCandidate>> biomeGroups = GetBiomeGroups(BiomeCandidates);

            List<BiomeParameters> biomes = GenerateBiomes(biomeGroups, random);
            
            var baseColor = PossibleBaseColors[Random.Range(0, PossibleBaseColors.Count)];

            var normalMap = PossibleNormalMaps[Random.Range(0, PossibleNormalMaps.Count)];
            var normalMapTile = Random.Range(NormalMapTileRange.x, NormalMapTileRange.y);
            var normalMapBlend = Random.Range(NormalMapBlendRange.x, NormalMapBlendRange.y);

            instance.InitializePlanet(biomes, CraterGenerationSettings, baseColor, normalMap, normalMapTile,
                normalMapBlend);

            return instance;
        }

        private List<BiomeParameters> GenerateBiomes(Dictionary<int, List<BiomeCandidate>> biomeGroups,
            System.Random random)
        {
            var biomes = new List<BiomeParameters>();
            if (biomeGroups.ContainsKey(0))
            {
                biomes = (from biomeCandidate in biomeGroups[0]
                    where random.NextDouble() < biomeCandidate.Probability
                    select biomeCandidate.BiomeType.GenerateBiomeParameters()).ToList();
            }
            
            biomes.AddRange(from biomeGroup in biomeGroups
                where biomeGroup.Key != 0
                select GetBiomeFromGroup(biomeGroup.Value, random)
                into biome
                where biome != null
                select biome);

            return biomes;
        }

        private BiomeParameters GetBiomeFromGroup(List<BiomeCandidate> biomeGroup, System.Random random)
        {
            var biomes = (from biomeCandidate in biomeGroup
                where random.NextDouble() < biomeCandidate.Probability
                select biomeCandidate.BiomeType.GenerateBiomeParameters()).ToList();

            return biomes.Count == 0 ? null : biomes[Random.Range(0, biomes.Count)];
        }

        private static Dictionary<int, List<BiomeCandidate>> GetBiomeGroups(List<BiomeCandidate> biomeCandidates)
        {
            var biomeGroups = new Dictionary<int, List<BiomeCandidate>>();

            foreach (var biomeCandidate in biomeCandidates)
            {
                if (!biomeGroups.ContainsKey(biomeCandidate.ExclusivityGroup))
                {
                    biomeGroups[biomeCandidate.ExclusivityGroup] = new List<BiomeCandidate>();
                }

                biomeGroups[biomeCandidate.ExclusivityGroup].Add(biomeCandidate);
            }

            return biomeGroups;
        }
    }
}