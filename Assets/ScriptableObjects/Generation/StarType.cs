using System.Collections.Generic;
using ProceduralPlanets.Generation;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "StarType", menuName = "Planetary Systems/Generation Types/Star Type")]
    public class StarType : CelestialBodyType<StarData>
    {
        [field: SerializeField] public List<StarType> CompatibleStarTypes { get; private set; }
        [field: SerializeField] public Vector2 StarNumberRange { get; private set; }
    }
}