using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.Movement;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using ProceduralPlanets.UI;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

namespace ProceduralPlanets.Generation
{
    public class SystemGenerator : MonoBehaviour
    {
        [SerializeField] private int seed;
        [SerializeField] private GenerationParameters generationParameters;

        [SerializeField] private GameObject starPrefab;
        [SerializeField] private GameObject planetPrefab;
        [SerializeField] private GameObject ringPrefab;
        [SerializeField] private GameObject atmospherePrefab;

        [SerializeField] private float orbitalSpeedInDegreesPerSecond = 10f;

        [SerializeField] private SystemMap systemMap;
        [SerializeField] private List<PlanetData> planets;

        private const string PrimeStarName = "Polaris";

        private readonly List<GameObject> _celestialBodies = new();

        public CelestialBodyGeneratorBase GetBodyByIndex(Vector2 index)
        {
            var mainBodyIndex = (int)index.x;
            return _celestialBodies[mainBodyIndex].GetComponent<CelestialBodyGeneratorBase>();
        }

        private void Start()
        {
            GenerateSystem();
        }

        public void GenerateSystem()
        {
            // if (Application.isPlaying) return;
            var random = new Random(seed);

            ClearExistingSystem();

            StarType primeStarType = generationParameters.StarTypes[random.Next(generationParameters.StarTypes.Length)];

            var starGenerationParameters = new CelestialBodyGenerationParameters<StarData, StarType>(starPrefab,
                primeStarType, seed, transform, PrimeStarName);

            GameObject primeStar = GenerateCelestialBody(starGenerationParameters);
            _celestialBodies.Add(primeStar);

            if (planets != null && planets.Count > 0)
            {
                GeneratePlanetsFromList(primeStar, random);
            }
            else
            {
                GeneratePlanets(primeStar, random);
            }

            if (!Application.isPlaying) return;
            systemMap.Generate(_celestialBodies.Select(body =>
                body.GetComponent<CelestialBodyGeneratorBase>()
                    .GetBodyData()).ToList());
        }

        private void ClearExistingSystem()
        {
            if (Application.isPlaying) return;
            while (transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                DestroyImmediate(child.gameObject);
            }
        }

        private void GeneratePlanets(GameObject primeStar, Random random)
        {
            StarType primeStarType = primeStar.GetComponent<StarGenerator>().BodyType;

            List<PlanetType> availablePlanetTypes =
                GetPlanetsCompatibleWithStar(generationParameters.PlanetTypes.ToList(), primeStarType);

            var maxPlanetCount = (int)primeStarType.PlanetNumberRange.y;
            var firstPlanetOrbitOffset = (float)(primeStarType.DistanceBetweenPlanetsRange.x * random.NextDouble());
            float currentOrbitRadius = primeStarType.PlanetOrbitRadiusRange.x + firstPlanetOrbitOffset;

            for (var i = 0; i < maxPlanetCount; i++)
            {
                if (currentOrbitRadius > primeStarType.PlanetOrbitRadiusRange.y) break;

                List<PlanetType> planetsWithCurrentOrbit =
                    GetPlanetsCompatibleWithOrbit(availablePlanetTypes, currentOrbitRadius);
                if (planetsWithCurrentOrbit.Count == 0) continue;

                PlanetType planetType = planetsWithCurrentOrbit[random.Next(planetsWithCurrentOrbit.Count)];

                var planetGenerationParameters =
                    new CelestialBodyGenerationParameters<PlanetData, PlanetType>(planetPrefab, planetType,
                        seed + i + 1, primeStar.transform, $"{PrimeStarName} {i + 1}");

                GameObject planet = GenerateCelestialBody(planetGenerationParameters);

                OrbitParameters planetOrbitParameters =
                    GenerateOrbitParameters(currentOrbitRadius, planetType.StarOrbitType, random);

                var planetOrbit = planet.GetComponent<OrbitalMovement>();
                planetOrbit.SetParameters(planetOrbitParameters);
                planetOrbit.MoveToStartingPosition(random);

                var offset = (float)(primeStarType.DistanceBetweenPlanetsRange.x + random.NextDouble() *
                    (primeStarType.DistanceBetweenPlanetsRange.y - primeStarType.DistanceBetweenPlanetsRange.x));
                currentOrbitRadius += offset;

                _celestialBodies.Add(planet);
            }
        }

        private void GeneratePlanetsFromList(GameObject primeStar, Random random)
        {
            StarType primeStarType = primeStar.GetComponent<StarGenerator>().BodyType;
            var firstPlanetOrbitOffset = (float)(primeStarType.DistanceBetweenPlanetsRange.x * random.NextDouble());
            float currentOrbitRadius = primeStarType.PlanetOrbitRadiusRange.x + firstPlanetOrbitOffset;
            
            for (var i = 0; i < planets.Count; i++)
            {
                if (currentOrbitRadius > primeStarType.PlanetOrbitRadiusRange.y) break;

                PlanetData planetData = planets[i];
                
                GameObject planet = InstantiatePlanetFromData(planetData, primeStar.transform, $"{PrimeStarName} {i + 1}");

                var orbitType = new OrbitType<StarType, StarData>(new Vector2(currentOrbitRadius, currentOrbitRadius),
                    Vector2.one,
                    Vector2.zero);
                
                OrbitParameters planetOrbitParameters =
                    GenerateOrbitParameters(currentOrbitRadius, orbitType, random);
                
                var planetOrbit = planet.GetComponent<OrbitalMovement>();
                planetOrbit.SetParameters(planetOrbitParameters);
                planetOrbit.MoveToStartingPosition(random);

                var offset = (float)(primeStarType.DistanceBetweenPlanetsRange.x + random.NextDouble() *
                    (primeStarType.DistanceBetweenPlanetsRange.y - primeStarType.DistanceBetweenPlanetsRange.x));
                currentOrbitRadius += offset;

                _celestialBodies.Add(planet);
            }
        }

        private GameObject InstantiatePlanetFromData(PlanetData planetData, Transform primeStarTransform, string planetName)
        {
            GameObject planet = Instantiate(planetPrefab, primeStarTransform);
            
            Instantiate(ringPrefab, planet.transform);
            Instantiate(atmospherePrefab, planet.transform);
            
            var planetGenerator = planet.GetComponent<PlanetGenerator>();
            planetGenerator.SetBodyData(planetData);
            
            planet.name = planetName;
            return planet;
        }

        GameObject GenerateCelestialBody<TData, TType>(CelestialBodyGenerationParameters<TData, TType> bodyParameters)
            where TData : CelestialBodyData
            where TType : CelestialBodyType<TData>
        {
            GameObject celestialBody;
            
            if (Application.isPlaying)
            {
                celestialBody = Instantiate(bodyParameters.Prefab, bodyParameters.Parent);
            }
            else
            {
                celestialBody = (GameObject)PrefabUtility.InstantiatePrefab(bodyParameters.Prefab);
            }

            var celestialBodyGenerator = celestialBody.GetComponent<CelestialBodyGenerator<TData, TType>>();
            celestialBodyGenerator.SetBodyType(bodyParameters.BodyType);
            celestialBodyGenerator.GenerateBodyData(bodyParameters.GenerationSeed);

            celestialBody.transform.SetParent(bodyParameters.Parent);
            celestialBody.name = bodyParameters.Name;
            celestialBodyGenerator.BodyData.name = bodyParameters.Name;
            return celestialBody;
        }

        private OrbitParameters GenerateOrbitParameters<TData, TType>(float mainRadius,
            OrbitType<TType, TData> orbitType, Random random)
            where TData : CelestialBodyData
            where TType : CelestialBodyType<TData>
        {
            var radiusRatio = (float)(orbitType.OrbitRatioRange.x + random.NextDouble() *
                (orbitType.OrbitRatioRange.y - orbitType.OrbitRatioRange.x));

            var rotation = (float)(random.NextDouble() * 360);

            var inclination = (float)(orbitType.OrbitInclinationRange.x + random.NextDouble() *
                (orbitType.OrbitInclinationRange.y - orbitType.OrbitInclinationRange.x));

            var orbitParameters = new OrbitParameters(mainRadius, radiusRatio, inclination, rotation,
                orbitalSpeedInDegreesPerSecond);
            return orbitParameters;
        }

        private List<PlanetType> GetPlanetsCompatibleWithOrbit(List<PlanetType> planetTypes, float orbitRadius)
        {
            return planetTypes.Where(planetType =>
                    planetType.StarOrbitType.OrbitRadiusRange.x <= orbitRadius &&
                    planetType.StarOrbitType.OrbitRadiusRange.y >= orbitRadius)
                .ToList();
        }

        private List<PlanetType> GetPlanetsCompatibleWithStar(List<PlanetType> planetTypes,
            CelestialBodyType<StarData> primeStarType)
        {
            return planetTypes.Where(planetType => planetType.StarOrbitType.ParentTypes.Contains(primeStarType))
                .ToList();
        }
    }
}