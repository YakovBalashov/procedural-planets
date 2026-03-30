using System.Collections.Generic;
using ProceduralPlanets.Generation;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "BiomeType", menuName = "Scriptable Objects/BiomeType")]
    public class BiomeType : ScriptableObject
    {
        [field: SerializeField] public float probability = 1.0f;
        [SerializeField] private List<Color> colors;
        [SerializeField] private int noiseType;
        [SerializeField] private float frequency;
        [SerializeField] private int octaves;
        [SerializeField] private float maskThreshold;
        
        public BiomeParameters GenerateBiomeParameters() 
        {
            var color = colors[Random.Range(0, colors.Count)];
            var biomeParameters = new BiomeParameters();
            biomeParameters.Initialize(color, noiseType, frequency, octaves, maskThreshold);
            return biomeParameters;
        }
    }
}
