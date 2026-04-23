using System.Collections.Generic;
using ProceduralPlanets.Generation;
using UnityEngine;
using Random = System.Random;
using ProceduralPlanets.Extensions;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "BiomeType", menuName = "Planetary Systems/Biome Type")]
    public class BiomeType : ScriptableObject
    {
        [SerializeField] BiomeParameters biomeParameters;
        [SerializeField] private List<Color> colors;
        private const float MaxOffset = 10000f;

        public BiomeParameters GenerateBiomeParameters(int seed)
        {
            var random = new Random(seed);

            var color = colors[random.Range(0, colors.Count)];
            var offset = new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat()) *
                         random.Range(0, MaxOffset);

            return biomeParameters.CreateCopy(offset, color);
        }
    }
}