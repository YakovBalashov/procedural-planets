using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "GenerationParameters", menuName = "Planetary Systems/Generation Parameters")]
    public class GenerationParameters : ScriptableObject
    {
        [field: SerializeField] public StarType[] StarTypes { get; private set; }
        [field: SerializeField] public PlanetType[] PlanetTypes { get; private set; }
    }
}
