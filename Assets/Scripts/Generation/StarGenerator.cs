using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(Light))]
    public class StarGenerator : CelestialBodyGenerator<StarData, StarType>
    {
        
    }
}