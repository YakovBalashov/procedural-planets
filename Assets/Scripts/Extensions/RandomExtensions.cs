using UnityEngine;

namespace ProceduralPlanets.Extensions
{
    public static class RandomExtensions
    {
        public static float Range(this System.Random random, Vector2 range)
        {
            return (float)(range.x + random.NextDouble() * (range.y - range.x));
        }
    }
}
