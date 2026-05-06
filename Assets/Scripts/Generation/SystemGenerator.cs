using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using ProceduralPlanets.Extensions;
using ProceduralPlanets.UI;

namespace ProceduralPlanets.Generation
{
    public abstract class SystemGenerator : MonoBehaviour
    {
        [SerializeField] protected int seed;
        [SerializeField] protected GameObject anchorPrefab;
        [SerializeField] protected GameObject starPrefab;
        [SerializeField] protected GameObject planetPrefab;
        [SerializeField] private SystemMap systemMap;

        protected List<GameObject> CelestialBodies = new();

        private const float MaxOffset = 10000f;
        private const int FirstCapitalLetterASCII = 65;

        public virtual void GenerateSystem()
        {
            systemMap.Generate(CelestialBodies);
        }

        public CelestialBodyGeneratorBase GetBodyByIndex(Vector2 index)
        {
            var mainBodyIndex = (int)index.x;
            var satelliteIndex = (int)index.y;

            if (satelliteIndex == 0) return CelestialBodies[mainBodyIndex].GetComponent<CelestialBodyGeneratorBase>();

            return CelestialBodies[mainBodyIndex].GetComponent<PlanetGenerator>().Moons[satelliteIndex - 1]
                .GetComponent<CelestialBodyGeneratorBase>();
        }


        public static char NumberToLetter(int index)
        {
            return (char)(FirstCapitalLetterASCII + index);
        }

        public static Vector3 GetOffset(Random random)
        {
            return new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat()) * random.Range(0, MaxOffset);
        }
    }
}