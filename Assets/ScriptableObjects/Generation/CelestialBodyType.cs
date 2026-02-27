using System.Collections.Generic;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    public abstract class CelestialBodyType<T> : ScriptableObject where T : CelestialBodyData
    {
        [SerializeField] protected Vector2 radiusRange;
        [SerializeField] protected List<NoiseSettings> cpuNoiseSettings;
        [SerializeField] protected List<Material> surfaceMaterial;
        private const float OffsetMultiplayer = 1000f;
        
        public virtual T CreateInstance(int seed)
        {
            var instance = CreateInstance<T>();
            var random = new System.Random(seed);

            float radius = radiusRange.x + (float)random.NextDouble() * (radiusRange.y - radiusRange.x);
            var copiedCPUNoiseSettings = new List<NoiseSettings>();
            foreach (var noiseSettings in cpuNoiseSettings)
            {
                var json = JsonUtility.ToJson(noiseSettings);
                var copiedNoiseSettings = JsonUtility.FromJson<NoiseSettings>(json);

                copiedNoiseSettings.Offset +=
                    new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble()) *
                    OffsetMultiplayer;
                copiedCPUNoiseSettings.Add(copiedNoiseSettings);
            }
            
            var material = surfaceMaterial[random.Next(surfaceMaterial.Count)];

            instance.Initialize(radius, copiedCPUNoiseSettings, material);
            return instance;
        }
    }
}
