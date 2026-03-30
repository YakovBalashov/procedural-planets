using System.Collections.Generic;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;
using Random = System.Random;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    public abstract class CelestialBodyType<T> : ScriptableObject where T : CelestialBodyData
    {
        [SerializeField] protected Vector2 radiusRange;
        [SerializeField] protected List<NoiseSettings> cpuNoiseSettings;
        [SerializeField] protected List<NoiseSettingsGPU> gpuNoiseSettings;
        private const float OffsetMultiplayer = 1000f;
        
        public virtual T CreateInstance(int seed)
        {
            var instance = CreateInstance<T>();
            var random = new System.Random(seed);

            float radius = radiusRange.x + (float)random.NextDouble() * (radiusRange.y - radiusRange.x);
            
            var copiedCPUNoiseSettings = DeepCopyNoiseSettings(cpuNoiseSettings, random);
            var copiedGPUNoiseSettings = DeepCopyNoiseSettings(gpuNoiseSettings, random);
            
            instance.Initialize(radius, copiedCPUNoiseSettings, copiedGPUNoiseSettings);
            return instance;
        }

        private List<TNoiseSettings> DeepCopyNoiseSettings<TNoiseSettings>(List<TNoiseSettings> sourceSettings, Random random) where TNoiseSettings : NoiseSettings
        {
            var copiedNoiseSettingsList = new List<TNoiseSettings>();
            foreach (var noiseSettings in sourceSettings)
            {
                var json = JsonUtility.ToJson(noiseSettings);
                var copiedNoiseSettings = JsonUtility.FromJson<TNoiseSettings>(json);

                copiedNoiseSettings.Offset +=
                    new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()) *
                    OffsetMultiplayer;
                copiedNoiseSettingsList.Add(copiedNoiseSettings);
            }

            return copiedNoiseSettingsList;
        }
    }
}
