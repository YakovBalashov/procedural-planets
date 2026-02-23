using System.Collections.Generic;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets
{
    [CreateAssetMenu(fileName = "CelestialBodyData", menuName = "Scriptable Objects/CelestialBodyData")]
    public class CelestialBodyData : ScriptableObject
    {
        [field: SerializeField] public float Radius { get; private set; } = 10f;
        
        [field: SerializeField] public List<NoiseSettings> CPUNoiseSettings { get; private set; } = new();
        
        [field: SerializeField] public List<NoiseSettings> GPUNoiseSettings { get; private set; } = new();

        [field: SerializeField] public Material SurfaceMaterial { get; private set; }
    }
}
