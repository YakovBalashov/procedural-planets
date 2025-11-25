using Unity.Mathematics;
using UnityEngine;

namespace ProceduralPlanets.Noise
{
    public class NoiseGenerator
    {
        private readonly NoiseSettings _settings;
        
        private static float NormaliseTo01(float value)
        {
            return (value + 1) * 0.5f;
        }
        
        public NoiseGenerator(NoiseSettings settings)
        {
            _settings = settings;
        }
        
        public float Evaluate(Vector3 point)
        {
            float noiseValue = 0f;

            for (int i = 0; i < _settings.Octaves; i++)
            {
                var frequency = _settings.BaseRoughness * Mathf.Pow(_settings.Lacunarity, i);
                var amplitude = Mathf.Pow(_settings.Persistence, i);
                
                Vector3 samplePoint = point * frequency + _settings.Offset;
                var perlinValue = NormaliseTo01(noise.cnoise(samplePoint));
                noiseValue += perlinValue * amplitude;
            }
            
            noiseValue = Mathf.Max(0, noiseValue - _settings.MinimumValue);
            
            return noiseValue * _settings.Strength;
        }
    }
}
