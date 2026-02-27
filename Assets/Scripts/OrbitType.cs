using System;
using System.Collections.Generic;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets
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
    }
}
