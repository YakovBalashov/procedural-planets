using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.Extensions;
using ProceduralPlanets.Movement;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using ProceduralPlanets.UI;
using UnityEngine;
using Random = System.Random;

namespace ProceduralPlanets.Generation
{
    public class SystemGenerator : MonoBehaviour
    {
        [SerializeField] private int seed;
        [SerializeField] private GenerationParameters generationParameters;

        [SerializeField] private GameObject anchorPrefab;
        [SerializeField] private GameObject starPrefab;
        [SerializeField] private GameObject planetPrefab;
        [SerializeField] private GameObject ringPrefab;
        [SerializeField] private GameObject atmospherePrefab;

        [SerializeField] private float orbitalSpeedInDegreesPerSecond = 10f;

        [SerializeField] private SystemMap systemMap;

        private const string PrimeStarName = "Polaris";
        private const int FirstCapitalLetterASCII = 65;
        private const float MaxOffset = 10000f;

        private readonly List<GameObject> _celestialBodies = new();

        public static char NumberToLetter(int index)
        {
            return (char)(FirstCapitalLetterASCII + index);
        }

        public static Vector3 GetOffset(Random random)
        {
            return new Vector3(random.NextFloat(), random.NextFloat(), random.NextFloat()) * random.Range(0, MaxOffset);
        }

        public CelestialBodyGeneratorBase GetBodyByIndex(Vector2 index)
        {
            var mainBodyIndex = (int)index.x;
            var satelliteIndex = (int)index.y;

            if (satelliteIndex == 0) return _celestialBodies[mainBodyIndex].GetComponent<CelestialBodyGeneratorBase>();

            return _celestialBodies[mainBodyIndex].GetComponent<PlanetGenerator>().Moons[satelliteIndex - 1]
                .GetComponent<CelestialBodyGeneratorBase>();
        }

        private void Start()
        {
            GenerateSystem();
        }

        public void GenerateSystem()
        {
            var random = new Random(seed);

            ClearExistingSystem();

            StarType primeStarType = generationParameters.StarTypes[random.Next(generationParameters.StarTypes.Length)];

            var starGenerationParameters = new CelestialBodyGenerationParameters<StarData, StarType>(starPrefab,
                primeStarType, seed, transform, PrimeStarName);

            GameObject primeStar = GenerateCelestialBody(starGenerationParameters);
            _celestialBodies.Add(primeStar);

            GeneratePlanets(primeStarType, primeStar, random);
            GenerateMoons(random);

            if (!Application.isPlaying) return;
            systemMap.Generate(_celestialBodies);
        }

        private void GenerateMoons(Random random)
        {
            var planets = _celestialBodies.Where(body => body.GetComponentInChildren<PlanetGenerator>()).ToList();

            foreach (var planet in planets)
            {
                var planetGenerator = planet.GetComponentInChildren<PlanetGenerator>();
                var planetType = planetGenerator.BodyType;

                var moonSatelliteParameters = new SatelliteGenerationParameters<PlanetType, PlanetData>(
                    planetType,
                    planet.transform,
                    type => type.PlanetOrbitType,
                    index => $"{planet.name} {NumberToLetter(index)}"
                );

                var moons = GenerateSatellites(moonSatelliteParameters, random);
                planetGenerator.Moons = moons;
            }
        }

        private void GeneratePlanets(StarType primeStarType, GameObject primeStar, Random random)
        {
            var planetSatelliteParameters = new SatelliteGenerationParameters<StarType, StarData>(
                primeStarType,
                primeStar.transform,
                type => type.StarOrbitType,
                index => $"{PrimeStarName} {index + 1}"
            );
            _celestialBodies.AddRange(GenerateSatellites(planetSatelliteParameters, random));
        }

        private void ClearExistingSystem()
        {
            if (Application.isPlaying) return;
            if (_celestialBodies.Count == 0) return;
            var primaryStarTransform = _celestialBodies[0].transform;
            while (primaryStarTransform.childCount > 0)
            {
                Transform child = primaryStarTransform.GetChild(0);
                DestroyImmediate(child.gameObject);
            }

            DestroyImmediate(primaryStarTransform.gameObject);
            _celestialBodies.Clear();
        }

        private List<GameObject> GenerateSatellites<TParent, TParentData>(
            SatelliteGenerationParameters<TParent, TParentData> parameters,
            Random random)
            where TParent : CelestialBodyType<TParentData>
            where TParentData : CelestialBodyData
        {
            List<PlanetType> availablePlanetTypes =
                GetPlanetsCompatibleWithParent(generationParameters.PlanetTypes.ToList(), parameters.ParentType,
                    parameters.OrbitSelector);

            SatelliteParameters satelliteParameters = parameters.ParentType.SatelliteParameters;

            var maxSatelliteCount = (int)satelliteParameters.NumberRange.y;
            var firstSatelliteOrbitOffset =
                random.Range(new Vector2(0, satelliteParameters.DistanceBetweenSatellitesRange.x));
            float currentOrbitRadius = satelliteParameters.OrbitRadiusRange.x + firstSatelliteOrbitOffset;

            List<GameObject> satellites = new List<GameObject>();

            var currentPlanetIndex = 0;
            while (currentOrbitRadius < satelliteParameters.OrbitRadiusRange.y &&
                   currentPlanetIndex < maxSatelliteCount)
            {
                var offset = random.Range(satelliteParameters.DistanceBetweenSatellitesRange);

                List<PlanetType> planetsWithCurrentOrbit =
                    GetPlanetsCompatibleWithOrbit(availablePlanetTypes, currentOrbitRadius, parameters.OrbitSelector);

                if (planetsWithCurrentOrbit.Count == 0)
                {
                    currentOrbitRadius += offset;
                    continue;
                }

                PlanetType satelliteType = planetsWithCurrentOrbit[random.Next(planetsWithCurrentOrbit.Count)];

                OrbitParameters satelliteOrbitParameters =
                    GenerateOrbitParameters(currentOrbitRadius, parameters.OrbitSelector(satelliteType), random);
                GameObject anchor = GenerateAnchor(satelliteOrbitParameters, parameters.ParentTransform, random);
                anchor.name = $"{parameters.NameGenerator(currentPlanetIndex)} Anchor";

                var satelliteGenerationParameters = new CelestialBodyGenerationParameters<PlanetData, PlanetType>(
                    planetPrefab, satelliteType,
                    seed + currentPlanetIndex + 1,
                    anchor.transform,
                    parameters.NameGenerator(currentPlanetIndex));
                GameObject satellite = GenerateCelestialBody(satelliteGenerationParameters);

                currentOrbitRadius += offset;

                satellites.Add(satellite);
                currentPlanetIndex += 1;
            }

            return satellites;
        }

        private GameObject GenerateAnchor(OrbitParameters parameters, Transform parent, Random random)
        {
            GameObject anchor = Instantiator.InstantiateGameObject(anchorPrefab, parent.parent);

            var orbitalMovement = anchor.GetComponent<OrbitalMovement>();
            orbitalMovement.SetParameters(parameters);
            orbitalMovement.MoveToStartingPosition(random);
            return anchor;
        }

        private GameObject InstantiatePlanetFromData(PlanetData planetData, Transform primeStarTransform,
            string planetName)
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
            GameObject celestialBody = Instantiator.InstantiateGameObject(bodyParameters.Prefab, bodyParameters.Parent);

            celestialBody.transform.localPosition = Vector3.zero;
            var celestialBodyGenerator = celestialBody.GetComponent<CelestialBodyGenerator<TData, TType>>();
            celestialBodyGenerator.SetBodyType(bodyParameters.BodyType);
            celestialBodyGenerator.GenerateBodyData(bodyParameters.GenerationSeed);

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

        private List<PlanetType> GetPlanetsCompatibleWithOrbit<TParent, TParentData>(
            List<PlanetType> planetTypes,
            float orbitRadius,
            Func<PlanetType, OrbitType<TParent, TParentData>> orbitSelector)
            where TParent : CelestialBodyType<TParentData>
            where TParentData : CelestialBodyData
        {
            return planetTypes.Where(planetType =>
                orbitSelector(planetType).OrbitRadiusRange.x <= orbitRadius &&
                orbitSelector(planetType).OrbitRadiusRange.y >= orbitRadius).ToList();
        }

        private List<PlanetType> GetPlanetsCompatibleWithParent<TParent, TParentData>(
            List<PlanetType> planetTypes,
            TParent parentType,
            Func<PlanetType, OrbitType<TParent, TParentData>> orbitSelector)
            where TParent : CelestialBodyType<TParentData>
            where TParentData : CelestialBodyData
        {
            return planetTypes
                .Where(planetType => orbitSelector(planetType).ParentTypes.Contains(parentType))
                .ToList();
        }
    }
}