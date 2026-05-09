using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProceduralPlanets.Extensions
{
    public static class RandomExtensions
    {
        public static float Range(this System.Random random, Vector2 range)
        {
            return (float)(range.x + random.NextDouble() * (range.y - range.x));
        }
        public static float Range(this System.Random random, float minValue, float maxValue)
        {
            return (float)(minValue + random.NextDouble() * (maxValue - minValue));
        }
        public static int Range(this System.Random random, int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }
        
        public static float NextFloat(this System.Random random)
        {
            return (float)random.NextDouble();
        }
        
        public static T GetRandomListElement<T>(this System.Random random, List<T> items, List<float> probabilities)
        {
            float totalProbability = probabilities.Sum();

            if (totalProbability <= 0f) return items[0];

            float randomPoint = random.NextFloat() * totalProbability;
    
            float currentSum = 0f;

            for (var i = 0; i < items.Count; i++)
            {
                if (probabilities[i] <= 0f) continue;

                currentSum += probabilities[i];

                if (randomPoint <= currentSum)
                {
                    return items[i];
                }
            }

            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (probabilities[i] > 0f) return items[i];
            }

            return items[^1];
        }
    }
}
