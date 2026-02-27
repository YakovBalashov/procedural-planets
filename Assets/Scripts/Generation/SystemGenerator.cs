using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
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

        [SerializeField] private float orbitalSpeedInDegreesPerSecond = 10f;

        private const string PrimeStarName = "Prime Star";

        public void GenerateSystem()
        {
            if (Application.isPlaying) return; 
            var random = new Random(seed);
            
            ClearExistingSystem();

            StarType primeStarType = generationParameters.StarTypes[random.Next(generationParameters.StarTypes.Length)];
            GameObject primeStar = GenerateCelestialBody<StarData, StarType>(starPrefab, primeStarType, seed);
            primeStar.transform.SetParent(transform);
            primeStar.name = PrimeStarName;

            GeneratePlanets(primeStar, random);
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
                GameObject planet =
                    GenerateCelestialBody<PlanetData, PlanetType>(planetPrefab, planetType, seed + i + 1);

                planet.transform.SetParent(primeStar.transform);
                planet.name = $"Planet {i + 1}";
                
                OrbitParameters planetOrbitParameters = GenerateOrbitParameters(currentOrbitRadius, planetType, random);
                
                var planetOrbit = planet.GetComponent<OrbitalMovement>();
                planetOrbit.SetParameters(planetOrbitParameters);
                planetOrbit.MoveToStartingPosition(random);

                var offset = (float)(primeStarType.DistanceBetweenPlanetsRange.x + random.NextDouble() *
                    (primeStarType.DistanceBetweenPlanetsRange.y - primeStarType.DistanceBetweenPlanetsRange.x));
                currentOrbitRadius += offset;
            }
        }

        private OrbitParameters GenerateOrbitParameters(float mainRadius, PlanetType planetType, Random random)
        {
            var radiusRatio = (float)(planetType.StarOrbitType.OrbitRatioRange.x + random.NextDouble() *
                (planetType.StarOrbitType.OrbitRatioRange.y - planetType.StarOrbitType.OrbitRatioRange.x));
            
            var rotation = (float)(random.NextDouble() * 360);
            
            var inclination = (float)(planetType.StarOrbitType.OrbitInclinationRange.x + random.NextDouble() *
                (planetType.StarOrbitType.OrbitInclinationRange.y - planetType.StarOrbitType.OrbitInclinationRange.x));

            var orbitParameters = new OrbitParameters
            {
                MainRadius = mainRadius,
                RadiusRatio = radiusRatio,
                Rotation = rotation,
                Inclination = inclination,
                SpeedInDegreesPerSecond = orbitalSpeedInDegreesPerSecond
            };
            return orbitParameters;
        }

        private List<PlanetType> GetPlanetsCompatibleWithOrbit(List<PlanetType> planetTypes, float orbitRadius)
        {
            return planetTypes.Where(planetType =>
                    planetType.StarOrbitType.OrbitRadiusRange.x <= orbitRadius &&
                    planetType.StarOrbitType.OrbitRadiusRange.y >= orbitRadius)
                .ToList();
        }

        GameObject GenerateCelestialBody<TData, TType>(GameObject prefab, TType bodyType, int generationSeed)
            where TData : CelestialBodyData
            where TType : CelestialBodyType<TData>
        {
            var celestialBody = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            var celestialBodyGenerator = celestialBody.GetComponent<CelestialBodyGenerator<TData, TType>>();
            celestialBodyGenerator.SetBodyType(bodyType);
            celestialBodyGenerator.GenerateBodyData(generationSeed);

            return celestialBody;
        }

        private List<PlanetType> GetPlanetsCompatibleWithStar(List<PlanetType> planetTypes,
            CelestialBodyType<StarData> primeStarType)
        {
            return planetTypes.Where(planetType => planetType.StarOrbitType.ParentTypes.Contains(primeStarType)).ToList();
        }
    }
}