using UnityEngine;

namespace ProceduralPlanets.Noise
{
    public enum FnlNoiseType
    {
        OpenSimplex2,
        OpenSimplex2S,
        Cellular,
        Perlin,
        ValueCubic,
        Value,
    }

    public enum FnlWarpType
    {
        OpenSimplex2,
        OpenSimplex2Reduced,
        BasicGrid,
    }

    [System.Serializable]
    public class FnlParameters
    {
        [field: SerializeField] public FnlNoiseType Type { get; private set; }
        [field: SerializeField] public float Frequency { get; private set; }
        [field: SerializeField] public int Octaves { get; private set; }
        [field: SerializeField] public Vector3 Offset { get; private set; }
        [field: SerializeField] public FnlWarpType WarpType { get; private set; }
        [field: SerializeField] public float WarpAmplitude { get; private set; }

        
        public void SetOffset(Vector3 offset)
        {
            Offset = offset;
        }
        public FnlParameters(FnlNoiseType type, float frequency, int octaves)
        {
            Type = type;
            Frequency = frequency;
            Octaves = octaves;
        }
    }
}