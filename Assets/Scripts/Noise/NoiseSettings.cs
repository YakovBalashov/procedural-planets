using UnityEngine;

namespace ProceduralPlanets.Noise
{
    [System.Serializable]
    public class NoiseSettings
    {
        [field: SerializeField] public bool Enabled { get; private set; } = true;
        [field: SerializeField] public float Strength { get; private set; } = 1f;
        
        [field: SerializeField, Range(0.01f, 10)] public float BaseRoughness { get; private set; } = 1f;
        [field: SerializeField, Range(0, 1)] public float Persistence { get; private set; } = 0.5f;
        [field: SerializeField] public float Lacunarity { get; private set; } = 2f;
        [field: SerializeField] public float MinimumValue { get; private set; } = 0f;
        
        [field: SerializeField, Range(1, 8)] public int Octaves { get; private set; } = 4;
        
        [field: SerializeField] public Vector3 Offset { get; private set; } = Vector3.zero;

        public NoiseSettingsGPU ToGPU()
        {
            return new NoiseSettingsGPU
            {
                Strength = Strength,
                BaseRoughness = BaseRoughness,
                Persistence = Persistence,
                Lacunarity = Lacunarity,
                MinimumValue = MinimumValue,
                Octaves = Octaves,
                Offset = Offset
            };
        }
        
    }
    
    public struct NoiseSettingsGPU
    {
        public float Strength;
        public float BaseRoughness;
        public float Persistence;
        public float Lacunarity;
        public float MinimumValue;
        public int Octaves;
        public Vector3 Offset;
    }
}