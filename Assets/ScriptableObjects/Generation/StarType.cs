using System.Collections.Generic;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "StarType", menuName = "Planetary Systems/Celestial Body Types/Star Type")]
    public class StarType : ScriptableObject
    {
        [field: SerializeField] public Vector2 PlanetNumberRange { get; private set; }
        [field: SerializeField] public List<StarType> CompatibleStarTypes { get; private set; }
        [field: SerializeField] public Vector2 StarNumberRange { get; private set; }
    }
}
