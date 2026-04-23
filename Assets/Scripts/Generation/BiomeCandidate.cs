using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [System.Serializable]
    public class BiomeCandidate
    {
        [field: SerializeField] public float Probability { get; private set; }
        [field: SerializeField] public int ExclusivityGroup { get; private set; }
        [field: SerializeField] public BiomeType BiomeType { get; private set; }
    }
}
