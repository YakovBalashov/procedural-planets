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
    }
}
