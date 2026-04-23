using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [System.Serializable]
    public class AtmosphereParameters
    {
        [field: SerializeField] public bool Enabled { get; private set; }
        [field: SerializeField] public Color Color { get; private set; }
        [field: SerializeField] public float RadiusMultiplier { get; private set; } = 1.05f;
        [field: SerializeField] public FnlParameters NoiseParameters { get; private set; }
        [field: SerializeField] public Vector2 NoiseRange { get; private set; } = new Vector2(-1f, 1f);
        [field: SerializeField] public float MinAlpha { get; private set; } = 0.1f;
        
        public AtmosphereParameters()
        {
            Enabled = false;
        }

        public AtmosphereParameters(bool enabled, Color color, float radiusMultiplier, FnlParameters noiseParameters,
            Vector2 noiseRange, float minAlpha)
        {
            Enabled = enabled;
            Color = color;
            RadiusMultiplier = radiusMultiplier;
            NoiseParameters = noiseParameters;
            NoiseRange = noiseRange;
            MinAlpha = minAlpha;
        }
    }
}