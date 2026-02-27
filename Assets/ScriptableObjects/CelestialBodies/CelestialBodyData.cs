using System.Collections.Generic;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets
{
    public abstract class CelestialBodyData : ScriptableObject
    {
        [field: SerializeField] public float Radius { get; protected set; } = 10f;
        [field: SerializeField] public List<NoiseSettings> CPUNoiseSettings { get; protected set; } = new();
        [field: SerializeField] public List<NoiseSettings> GPUNoiseSettings { get; protected set; } = new();
        [field: SerializeField] public Material SurfaceMaterial { get; protected set; }
        
        public void Initialize(float radius, List<NoiseSettings> noiseSettings, Material surfaceMaterial)
        {
            Radius = radius;
            CPUNoiseSettings = noiseSettings;
            SurfaceMaterial = surfaceMaterial;
        }
    }
}
