using System;
using System.Linq.Expressions;
using ProceduralPlanets.Movement;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public struct SatelliteGenerationParameters<TParent, TParentData>
        where TParent : CelestialBodyType<TParentData>
        where TParentData : CelestialBodyData
    {
        public readonly TParent ParentType;
        public readonly Transform ParentTransform;

        public readonly Func<PlanetType, OrbitType<TParent, TParentData>> OrbitSelector;
        public readonly Func<int, string> NameGenerator;

        public SatelliteGenerationParameters(
            TParent parentType,
            Transform parentTransform,
            Func<PlanetType, OrbitType<TParent, TParentData>> orbitSelector,
            Func<int, string> nameGenerator)
        {
            ParentType = parentType;
            ParentTransform = parentTransform;
            OrbitSelector = orbitSelector;
            NameGenerator = nameGenerator;
        }
    }
}