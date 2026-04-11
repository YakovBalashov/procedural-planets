using UnityEngine;
using UnityEngine.Serialization;

namespace ProceduralPlanets.Noise
{
    public enum FnlType
    {
        OpenSimplex2,
        OpenSimplex2S,
        Cellular,
        Perlin,
        ValueCubic,
        Value,
    }

    [System.Serializable]
    public class FnlParameters
    {
        [field: SerializeField] public FnlType Type { get; private set; }
        [field: SerializeField] public float Frequency { get; private set; }
        [field: SerializeField] public int Octaves { get; private set; }

        public FnlParameters(FnlType type, float frequency, int octaves)
        {
            Type = type;
            Frequency = frequency;
            Octaves = octaves;
        }
    }
}