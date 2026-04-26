using System.Collections.Generic;
using ProceduralPlanets.Generation;
using UnityEngine;
using Random = System.Random;
using ProceduralPlanets.Extensions;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "BiomeType", menuName = "Planetary Systems/Generation Types/Biome Type")]
    public class BiomeType : ScriptableObject
    {
        [SerializeField] BiomeParameters biomeParameters;
        [SerializeField] private List<Color> colors;
        [SerializeField] private List<BiomeColors> biomeColors;
        private const float MaxOffset = 10000f;

        public BiomeParameters GenerateBiomeParameters(int seed)
        {
            var random = new Random(seed);

            var biomeColor = biomeColors[random.Next(0, biomeColors.Count)];
            var offset = new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat()) *
                         random.Range(0, MaxOffset);

            return biomeParameters.CreateCopy(offset, biomeColor);
        }
    }
}