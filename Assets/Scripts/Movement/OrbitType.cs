using System;
using System.Collections.Generic;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Movement
{
    [Serializable]
    public struct OrbitType<TType, TData> 
        where TType : CelestialBodyType<TData>
        where TData : CelestialBodyData
    {
        [field: SerializeField] public List<TType> ParentTypes { get; private set; }
        [field: SerializeField] public Vector2 OrbitRadiusRange { get; private set; }
        [field: SerializeField] public Vector2 OrbitRatioRange { get; private set; }
        [field: SerializeField] public Vector2 OrbitInclinationRange { get; private set; }

        public OrbitType(Vector2 orbitRadiusRange, Vector2 orbitRatioRange,
            Vector2 orbitInclinationRange)
        {
            ParentTypes = new List<TType>();
            OrbitRadiusRange = orbitRadiusRange;
            OrbitRatioRange = orbitRatioRange;
            OrbitInclinationRange = orbitInclinationRange;
        }
    }
}
