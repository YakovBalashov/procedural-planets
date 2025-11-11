using Unity.Mathematics;
using UnityEngine;

namespace ProceduralPlanets
{
    public class NoiseMap
    {
        private const float MinScale = 0.0001f;
        
        private const int MinOffset = -100000;
        private const int MaxOffset = 100000;

        private readonly float _scale;
        private readonly int _octaves;
        private readonly float _persistence;
        private readonly float _lacunarity;

        private readonly Vector3[] _octaveOffsets;
        
        public NoiseMap(int seed, float scale, int octaves, float persistence, float lacunarity)
        {
            _scale = Mathf.Max(scale, MinScale);
            _octaves = octaves;
            _persistence = persistence;
            _lacunarity = lacunarity;
            
            var pseudoRandomGenerator = new System.Random(seed);
            
            _octaveOffsets = new Vector3[_octaves];
            for (var i = 0; i < _octaves; i++)
            {
                float offsetX = pseudoRandomGenerator.Next(MinOffset, MaxOffset);
                float offsetY = pseudoRandomGenerator.Next(MinOffset, MaxOffset);
                float offsetZ = pseudoRandomGenerator.Next(MinOffset, MaxOffset);
                
                _octaveOffsets[i] = new Vector3(offsetX, offsetY, offsetZ);
            }
        }
        
        public float[] GenerateNoiseMap(Vector3[] points)
        {
            var noiseMap = new float[points.Length];
            var minValue = float.MaxValue;
            var maxValue = float.MinValue;
            
            for (var i = 0; i < points.Length; i++)
            {
                noiseMap[i] = GenerateNoise(points[i]);

                if (noiseMap[i] < minValue) minValue = noiseMap[i];
                
                if (noiseMap[i] > maxValue) maxValue = noiseMap[i];
            }
            
            for (var i = 0; i < noiseMap.Length; i++)
            {
                noiseMap[i] = Mathf.InverseLerp(minValue, maxValue, noiseMap[i]) * 2 - 1;
            }
            
            return noiseMap;
        }
        
        public float GenerateNoise(Vector3 point)
        {
            var noiseHeight = 0f;
            for (var i = 0; i < _octaves; i++)
            {
                var frequency = Mathf.Pow(_lacunarity, i);
                var amplitude = Mathf.Pow(_persistence, i);

                float3 samplePoint = point * frequency / _scale + _octaveOffsets[i];
                var perlinValue = noise.cnoise(samplePoint);
                noiseHeight += perlinValue * amplitude;
            }
            return noiseHeight;
        }
        
        
        public Texture2D GenerateNoiseTexture(int width, int height)
        {
            var noiseTexture = new Texture2D(width, height);
            Vector3[] points = new Vector3[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    points[y * width + x] = new Vector3(x, y, 0);
                }
            }
            
            float[] noiseValues = GenerateNoiseMap(points);
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float noiseValue = noiseValues[y * width + x] * 0.5f + 0.5f;
                    Color color = new Color(noiseValue, noiseValue, noiseValue);
                    noiseTexture.SetPixel(x, y, color);
                }
            }
            
            noiseTexture.Apply();
            return noiseTexture;
        }
    }
}
