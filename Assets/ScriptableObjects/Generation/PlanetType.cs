using System.Collections.Generic;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "PlanetType", menuName = "Planetary Systems/Celestial Body Types/Planet Type")]
    public class PlanetType : CelestialBodyType<PlanetData>
    {
        [field: SerializeField] public List<StarType> ParentStarTypes { get; private set; }
        [field: SerializeField] public Vector2 StarOrbitRadiusRange { get; private set; }
        [field: SerializeField] public Vector2 StarOrbitRatioRange { get; private set; }

        [field: SerializeField] public List<PlanetType> ParentPlanetTypes { get; private set; }
        [field: SerializeField] public Vector2 PlanetOrbitRadiusRange { get; private set; }
        [field: SerializeField] public Vector2 PlanetOrbitRatioRange { get; private set; }

        [field: SerializeField] public Vector2 MoonNumberRange { get; private set; }
    }
}