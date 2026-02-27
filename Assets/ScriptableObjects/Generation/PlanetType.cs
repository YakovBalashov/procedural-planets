using System.Collections.Generic;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "PlanetType", menuName = "Planetary Systems/Celestial Body Types/Planet Type")]
    public class PlanetType : CelestialBodyType<PlanetData>
    {
        [field: SerializeField] public OrbitType<StarType, StarData> StarOrbitType { get; private set; }
        [field: SerializeField] public OrbitType<PlanetType, PlanetData> PlanetOrbitType { get; private set; }

        [field: SerializeField] public Vector2 MoonNumberRange { get; private set; }
    }
}