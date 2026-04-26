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
        
        public static T GetRandomListElement<T> (this System.Random random, List<T> items, List<float> probabilities)
        {
            var totalProbability = probabilities.Sum();

            var randomPoint = random.NextFloat() * totalProbability;

            for (var i = 0; i < items.Count; i++)
            {
                if (randomPoint < probabilities[i]) return items[i];
                randomPoint -= probabilities[i];
            }
            return items[^1];
        }
    }
}
