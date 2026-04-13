using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [System.Serializable]
    public class RingParameters
    {
        [field: SerializeField] public bool Enabled { get; private set; }
        [field: SerializeField] public float InnerRadius { get; private set; } = 1f;
        [field: SerializeField] public float OuterRadius { get; private set; } = 2f;
        [field: SerializeField] public int SegmentCount { get; private set; } = 100;
        [field: SerializeField] public FnlParameters NoiseParameters { get; private set; }
        [field: SerializeField] public Color RingColor { get; private set; }
    }
}