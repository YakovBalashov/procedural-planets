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
    }
}