using System.Collections.Generic;
using ProceduralPlanets.Extensions;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "AtmosphereType", menuName = "Planetary Systems/Generation Types/Atmosphere Type")]
    public class AtmosphereType : ScriptableObject
    {
        [field: SerializeField] public float RadiusMultiplier { get; private set; } = 1.05f;
        [field: SerializeField] public FnlParameters NoiseParameters { get; private set; }
        [field: SerializeField] public Vector2 NoiseRangeMinRange { get; private set; } = new Vector2(-1f, -0.8f);
        [field: SerializeField] public Vector2 NoiseRangeMaxRange { get; private set; } = new Vector2(0.8f, 1f);
        [field: SerializeField] public Vector2 MinAlphaRange { get; private set; } = new Vector2(0.05f, 0.15f);
        [field: SerializeField] public List<Color> Colors { get; private set; }
        
        public AtmosphereParameters GenerateAtmosphereParameters(int seed)
        {
            var random = new System.Random(seed);
            
            var color = Colors[random.Range(0, Colors.Count)];
            var noiseRangeMin = random.Range(NoiseRangeMinRange);
            var noiseRangeMax = random.Range(NoiseRangeMaxRange);
            var minAlpha = random.Range(MinAlphaRange);
            
            NoiseParameters.SetOffset(SystemGenerator.GetOffset(random));
            
            return new AtmosphereParameters(true, color, RadiusMultiplier, NoiseParameters, new Vector2(noiseRangeMin, noiseRangeMax), minAlpha);
        }
    }
}
