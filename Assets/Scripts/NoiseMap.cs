using System;
using Unity.Mathematics;
using UnityEngine;

namespace ProceduralPlanets
{
    public class NoiseMap
    {
        private const float MinScale = 0.0001f;
        
        private readonly int _octaves;
        private readonly float _persistence;
        private readonly float _lacunarity;
        
        public NoiseMap(float scale, int octaves, float persistence, float lacunarity)
        {
            _octaves = octaves;
            _persistence = persistence;
            _lacunarity = lacunarity;
        }
        
        public float GenerateNoise(Vector3 point)
        {
            return 1f;
        }
    }
}
