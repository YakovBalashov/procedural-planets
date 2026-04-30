using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.Extensions;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    public abstract class CelestialBodyType<T> : ScriptableObject where T : CelestialBodyData
    {
        [SerializeField] protected Vector2 radiusRange;
        [SerializeField] protected List<NoiseSettingsGPU> gpuNoiseSettings;
        [field: SerializeField] public List<Color> PossibleBaseColors { get; private set; }
        [field: SerializeField] public List<Texture2D> PossibleNormalMaps { get; private set; }
        [field: SerializeField] public Vector2 NormalMapTileRange { get; private set; }
        [field: SerializeField] public Vector2 NormalMapBlendRange { get; private set; }
        [field: SerializeField] public List<BiomeCandidate> BiomeCandidates { get; private set; }
        [field: SerializeField] public SatelliteParameters SatelliteParameters { get; private set; }
        [field: SerializeField] public float RadiusToPlayerOrbitRation { get; private set; } = 1.5f;
        private const float OffsetMultiplayer = 1000f;

        public virtual T CreateInstance(int seed)
        {
            var instance = CreateInstance<T>();
            var random = new System.Random(seed);

            float radius = radiusRange.x + (float)random.NextDouble() * (radiusRange.y - radiusRange.x);

            var copiedGPUNoiseSettings = DeepCopyNoiseSettings(gpuNoiseSettings, random);

            Dictionary<int, List<BiomeCandidate>> biomeGroups = GetBiomeGroups(BiomeCandidates);

            var baseColor = PossibleBaseColors[random.Range(0, PossibleBaseColors.Count)];

            List<BiomeParameters> biomes = GenerateBiomes(biomeGroups, random, baseColor);

            var normalMap = PossibleNormalMaps[random.Range(0, PossibleNormalMaps.Count)];
            var normalMapTile = random.Range(NormalMapTileRange.x, NormalMapTileRange.y);
            var normalMapBlend = random.Range(NormalMapBlendRange.x, NormalMapBlendRange.y);

            instance.Initialize(radius, copiedGPUNoiseSettings, biomes, baseColor, normalMap,
                normalMapTile, normalMapBlend, RadiusToPlayerOrbitRation);
            return instance;
        }

        private List<TNoiseSettings> DeepCopyNoiseSettings<TNoiseSettings>(List<TNoiseSettings> sourceSettings,
            Random random) where TNoiseSettings : NoiseSettings
        {
            var copiedNoiseSettingsList = new List<TNoiseSettings>();
            foreach (var noiseSettings in sourceSettings)
            {
                var json = JsonUtility.ToJson(noiseSettings);
                var copiedNoiseSettings = JsonUtility.FromJson<TNoiseSettings>(json);

                copiedNoiseSettings.Offset +=
                    new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()) *
                    OffsetMultiplayer;
                copiedNoiseSettingsList.Add(copiedNoiseSettings);
            }

            return copiedNoiseSettingsList;
        }

        private List<BiomeParameters> GenerateBiomes(Dictionary<int, List<BiomeCandidate>> biomeGroups,
            System.Random random, Color baseColor)
        {
            var biomes = new List<BiomeParameters>();
            if (biomeGroups.ContainsKey(0))
            {
                biomes = (from biomeCandidate in biomeGroups[0]
                        where random.NextDouble() < biomeCandidate.Probability
                        select biomeCandidate.BiomeType.GenerateBiomeParameters(random.Next(0, int.MaxValue),
                            baseColor))
                    .ToList();
            }

            biomes.AddRange(from biomeGroup in biomeGroups
                where biomeGroup.Key != 0
                select GetBiomeFromGroup(biomeGroup.Value, random, baseColor)
                into biome
                where biome != null
                select biome);

            return biomes;
        }

        private BiomeParameters GetBiomeFromGroup(List<BiomeCandidate> biomeGroup, System.Random random,
            Color baseColor)
        {
            var biomes = (from biomeCandidate in biomeGroup
                    where random.NextDouble() < biomeCandidate.Probability
                    select biomeCandidate.BiomeType.GenerateBiomeParameters(random.Next(0, int.MaxValue), baseColor))
                .ToList();

            return biomes.Count == 0 ? null : biomes[random.Range(0, biomes.Count)];
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