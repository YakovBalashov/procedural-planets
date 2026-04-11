using System.Collections.Generic;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "BiomeType", menuName = "Planetary Systems/Biome Type")]
    public class BiomeType : ScriptableObject
    {
        [field: SerializeField] public float probability = 1.0f;
        [SerializeField] private List<Color> colors;
        [SerializeField] private FnlType noiseType;
        [SerializeField] private float frequency;
        [SerializeField] private int octaves;
        [SerializeField] private float maskThreshold;
        [SerializeField] private float blendFactor;

        public BiomeParameters GenerateBiomeParameters()
        {
            var color = colors[Random.Range(0, colors.Count)];
            var biomeParameters = new BiomeParameters(color, new FnlParameters(noiseType, frequency, octaves),
                maskThreshold, blendFactor);
            return biomeParameters;
        }
    }
}