using System.Collections.Generic;
using ProceduralPlanets.Movement;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [System.Serializable]
    public struct LocalSystemData
    {
        [field: SerializeField] public PlanetAndOrbitData Planet { get; private set; }
        [field: SerializeField] public List<PlanetAndOrbitData> Moons { get; private set; }
    }

    [System.Serializable]
    public struct PlanetAndOrbitData
    {
        [field: SerializeField] public PlanetData PlanetData { get; private set; }
        [field: SerializeField] public OrbitParameters OrbitParameters { get; private set; }
    }
}