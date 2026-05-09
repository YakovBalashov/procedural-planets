using System;
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
        [SerializeField] private float timeMultiplier = 1f;

        public static int CurrentTimeExponent { get; private set; } = 0;
        public static float TimeMultiplier => Mathf.Pow(10, CurrentTimeExponent);
        public const int MinTimeExponent = -2;
        public const int MaxTimeExponent = 6;

        public static MessageText MessageText;
        
        protected readonly List<GameObject> CelestialBodies = new();

        private const float MaxOffset = 10000f;
        private const int FirstCapitalLetterASCII = 65;

        private void Awake()
        {
            seed = new Random().Next();
            timeMultiplier = 1f;
            GenerateSystem();
        }
        
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

        private void OnValidate()
        {
            if (Mathf.Approximately(timeMultiplier, TimeMultiplier)) return;
            SetTimeMultiplier(timeMultiplier);
        }
        
        public static void SetTimeMultiplier(float multiplier)
        {
            var newTimeExponent = Mathf.Log10(multiplier);
            CurrentTimeExponent = Mathf.Clamp(Mathf.RoundToInt(newTimeExponent), MinTimeExponent, MaxTimeExponent);
        }
        
        public static void SetTimeExponent(int exponent)
        {
            CurrentTimeExponent = Mathf.Clamp(exponent, MinTimeExponent, MaxTimeExponent);
        }
    }
}