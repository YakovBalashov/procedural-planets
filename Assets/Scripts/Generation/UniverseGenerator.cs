using System;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public class UniverseGenerator : MonoBehaviour
    {
        [field: SerializeField] public GenerationParameters GenerationParameters { get; private set; }

        private void Awake()
        {
            var test = new PlanetData();
            
        }
    }
}
