using UnityEngine;

namespace ProceduralPlanets.Noise
{
    [System.Serializable]
    public class NoiseSettingsGPU : NoiseSettings
    {
        [field: SerializeField] public int FnlSeed { get; private set; } = 0;
        [field: SerializeField] public int FnlNoiseType { get; private set; } = 0;

        public NoiseSettingsGPUStruct ToGPU()
        {
            return new NoiseSettingsGPUStruct
            {
                Strength = Strength,
                BaseRoughness = BaseRoughness,
                Persistence = Persistence,
                Lacunarity = Lacunarity,
                MinimumValue = MinimumValue,
                Octaves = Octaves,
                Offset = Offset,
                FnlNoiseType = FnlNoiseType,
                FnlSeed = FnlSeed,
            };
        }
    }

    public struct NoiseSettingsGPUStruct
    {
        public float Strength;
        public float BaseRoughness;
        public float Persistence;
        public float Lacunarity;
        public float MinimumValue;
        public int Octaves;
        public Vector3 Offset;
        public int FnlSeed;
        public int FnlNoiseType;
    }
}