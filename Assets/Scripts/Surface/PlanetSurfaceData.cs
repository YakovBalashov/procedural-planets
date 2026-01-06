using System.Collections.Generic;
using ProceduralPlanets.Noise;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProceduralPlanets.Surface
{
    [CreateAssetMenu(fileName = "PlanetSurfaceData", menuName = "Planet Data/PlanetSurfaceData")]
    public class PlanetSurfaceData : ScriptableObject
    {
        [field: SerializeField, Range(1.0f, 100f)]
        public float Radius { get; private set; } = 10f;

        [field: FormerlySerializedAs("<NoiseSettings>k__BackingField")] [field: SerializeField]
        public List<NoiseSettings> CPUNoiseSettings { get; private set; } = new();
        
        [field: SerializeField]
        public List<NoiseSettings> GPUNoiseSettings { get; private set; } = new();

        [field: SerializeField] public Material PlanetMaterial { get; private set; }
    }
}