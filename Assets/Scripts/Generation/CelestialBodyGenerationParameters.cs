using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    public struct CelestialBodyGenerationParameters<TData, TType>
        where TData : CelestialBodyData
        where TType : CelestialBodyType<TData>
    {
        public readonly GameObject Prefab;
        public readonly TType BodyType;
        public readonly int GenerationSeed;
        public readonly Transform Parent;
        public readonly string Name;

        public CelestialBodyGenerationParameters(GameObject prefab, TType bodyType, int generationSeed, Transform parent, string name)
        {
            Prefab = prefab;
            BodyType = bodyType;
            GenerationSeed = generationSeed;
            Parent = parent;
            Name = name;
        }
    }
}