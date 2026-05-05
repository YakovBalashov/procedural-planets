using UnityEngine;

namespace ProceduralPlanets.LOD
{
    [System.Serializable]
    public struct LODLevel
    {
        [field: SerializeField] public int Resolution { get; private set; }
        [field: SerializeField] public float ScreenCoverageThreshold { get; private set; }
    }
}