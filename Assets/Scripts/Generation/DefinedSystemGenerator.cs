using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;
using Random = System.Random;
using ProceduralPlanets.Extensions;
using ProceduralPlanets.Movement;


namespace ProceduralPlanets.Generation
{
    public class DefinedSystemGenerator : SystemGenerator
    {
        [SerializeField] private StarData starData;
        [SerializeField] private List<LocalSystemData> localSystems;
        [SerializeField] private string starName;
        [SerializeField] private float radiusToOrbitMultiplier = 4f;

        public override void GenerateSystem()
        {
            var star = Instantiator.InstantiateGameObject(starPrefab, transform);
            var starGenerator = star.GetComponent<StarGenerator>();
            starGenerator.SetBodyData(starData);
            star.name = starName;

            var random = new Random(seed);

            var planetGenerationParameters = new SatelliteGenerationParameters
            {
                Satellites = localSystems.Select(localSystem => localSystem.Planet).ToList(),
                ParentTransform = star.transform,
            };

            CelestialBodies = GenerateDefinedSatellites(planetGenerationParameters, random);

            foreach (var planet in CelestialBodies)
            {
                var planetGenerator = planet.GetComponent<PlanetGenerator>();
                var localSystem = localSystems.First(ls => ls.Planet.PlanetData == planetGenerator.GetBodyData());
                var data = planetGenerator.GetBodyData();

                var moonGenerationParameters = new SatelliteGenerationParameters
                {
                    Satellites = localSystem.Moons,
                    ParentTransform = planet.transform,
                };

                var moons = GenerateDefinedSatellites(moonGenerationParameters, random);
                planetGenerator.Moons = moons;
            }

            base.GenerateSystem();
        }

        private List<GameObject> GenerateDefinedSatellites(SatelliteGenerationParameters parameters, Random random)
        {
            var satellites = new List<GameObject>();

            foreach (var planetAndOrbitData in parameters.Satellites)
            {
                var satelliteName = planetAndOrbitData.PlanetData.name;

                var anchor = Instantiator.InstantiateGameObject(anchorPrefab, parameters.ParentTransform);
                var anchorOrbit = anchor.GetComponent<OrbitalMovement>();
                anchorOrbit.SetParameters(planetAndOrbitData.OrbitParameters);

                var satellite = Instantiator.InstantiateGameObject(planetPrefab, anchor.transform);
                satellite.transform.localPosition = Vector3.zero;
                var satelliteGenerator = satellite.GetComponent<PlanetGenerator>();
                satelliteGenerator.SetBodyData(planetAndOrbitData.PlanetData);
                satellite.name = satelliteName;

                anchorOrbit.MoveToStartingPosition(random);

                satellites.Add(satellite);
            }

            return satellites;
        }

        private struct SatelliteGenerationParameters
        {
            public List<PlanetAndOrbitData> Satellites;
            public Transform ParentTransform;
        }
    }
}