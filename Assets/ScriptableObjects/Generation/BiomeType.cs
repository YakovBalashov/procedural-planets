using System.Collections.Generic;
using ProceduralPlanets.Generation;
using UnityEngine;
using Random = System.Random;
using ProceduralPlanets.Extensions;
using TMPro;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "BiomeType", menuName = "Planetary Systems/Generation Types/Biome Type")]
    public class BiomeType : ScriptableObject
    {
        [SerializeField] BiomeParameters biomeParameters;
        [SerializeField] private List<BiomeColors> biomeColors;
        [SerializeField] private bool useColorBasedProbabilities;
        
        private const float MaxOffset = 10000f;
        private const float Bias = 0.01f;

        public BiomeParameters GenerateBiomeParameters(int seed, Color baseColor)
        {
            var random = new Random(seed);

            BiomeColors biomeColor;
            
            if (useColorBasedProbabilities)
            {
                List<float> colorProbabilities = CalculateColorProbabilities(biomeColors, baseColor);
                biomeColor = random.GetRandomListElement(biomeColors, colorProbabilities);
            } else
            {
                biomeColor = biomeColors[random.Range(0, biomeColors.Count)];
            }
            
            var offset = new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat()) *
                         random.Range(0, MaxOffset);

            return biomeParameters.CreateCopy(offset, biomeColor);
        }

        private List<float> CalculateColorProbabilities(List<BiomeColors> biomeColorsList, Color baseColor)
        {
            var probabilities = new List<float>();
            float totalWeight = 0f;

            foreach (var biomeColor in biomeColorsList)
            {
                var distance = Vector3.Distance(new Vector3(biomeColor.baseColor.r, biomeColor.baseColor.g, biomeColor.baseColor.b),
                    new Vector3(baseColor.r, baseColor.g, baseColor.b));
                var weight = 1f / (distance + Bias);
                probabilities.Add(weight);
                totalWeight += weight;
            }

            for (var i = 0; i < probabilities.Count; i++) probabilities[i] /= totalWeight;

            return probabilities;
        }
    }
}