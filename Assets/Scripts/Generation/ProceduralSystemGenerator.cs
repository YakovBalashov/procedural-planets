using System;
using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.Extensions;
using ProceduralPlanets.Movement;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;
using Random = System.Random;

namespace ProceduralPlanets.Generation
{
    public class ProceduralSystemGenerator : SystemGenerator
    {
        [SerializeField] private GenerationParameters generationParameters;

        private const string PrimeStarName = "Polaris";

        private void Awake()
        {
            GenerateSystem();
        }

        public override void GenerateSystem()
        {
            var random = new Random(seed);

            ClearExistingSystem();

            StarType primeStarType = generationParameters.StarTypes[random.Next(generationParameters.StarTypes.Length)];

            var starGenerationParameters = new CelestialBodyGenerationParameters<StarData, StarType>(starPrefab,
                primeStarType, seed, transform, PrimeStarName);

            GameObject primeStar = GenerateCelestialBody(starGenerationParameters);
            CelestialBodies.Add(primeStar);

            GeneratePlanets(primeStarType, primeStar, random);
            GenerateMoons(random);

            if (!Application.isPlaying) return;
            base.GenerateSystem();
        }

        private void GenerateMoons(Random random)
        {
            var planets = CelestialBodies.Where(body => body.GetComponentInChildren<PlanetGenerator>()).ToList();

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
            CelestialBodies.AddRange(GenerateSatellites(planetSatelliteParameters, random));
        }

        private void ClearExistingSystem()
        {
            CelestialBodies.Clear();
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

            var velocity = random.Range(orbitType.VelocityRange);

            var orbitParameters = new OrbitParameters(mainRadius, radiusRatio, inclination, rotation,
                velocity);
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