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
        [field: SerializeField] public Vector2 NoiseRange { get; private set; } = new Vector2(-1f, 1f);
        [field: SerializeField] public Color RingColor { get; private set; }

        public RingParameters(bool enabled, float innerRadius, float outerRadius, int segmentCount, FnlParameters noiseParameters, Color ringColor, Vector2 noiseRange)
        {
            Enabled = enabled;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            SegmentCount = segmentCount;
            NoiseParameters = noiseParameters;
            RingColor = ringColor;
            NoiseRange = noiseRange;
        }

        public RingParameters()
        {
            Enabled = false;
        }
    }
}