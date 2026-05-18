using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;
using Random = System.Random;
using ProceduralPlanets.Movement;


namespace ProceduralPlanets.Generation
{
    public class DefinedSystemGenerator : SystemGenerator
    {
        [SerializeField] private StarData starData;
        [SerializeField] private List<LocalSystemData> localSystems;
        [SerializeField] private string starName;

        public override void GenerateSystem()
        {
            CelestialBodies.Clear();
            var star = Instantiator.InstantiateGameObject(starPrefab, transform);
            var starGenerator = star.GetComponent<StarGenerator>();
            starGenerator.SetBodyData(starData);
            star.name = starName;
            CelestialBodies.Add(star);

            var random = new Random(seed);

            var planetGenerationParameters = new SatelliteGenerationParameters
            {
                Satellites = localSystems.Select(localSystem => localSystem.Planet).ToList(),
                ParentTransform = star.transform,
            };

            CelestialBodies.AddRange(GenerateDefinedSatellites(planetGenerationParameters, random));

            foreach (var planet in CelestialBodies)
            {
                var planetGenerator = planet.GetComponent<PlanetGenerator>();
                if (!planetGenerator) continue;
                var localSystem = localSystems.First(ls => ls.Planet.PlanetData == planetGenerator.GetBodyData());

                var moonGenerationParameters = new SatelliteGenerationParameters
                {
                    Satellites = localSystem.Moons,
                    ParentTransform = planet.transform,
                };

                var moons = GenerateDefinedSatellites(moonGenerationParameters, random);
                planetGenerator.Moons = moons;
            }
            if (!Application.isPlaying) return;
            base.GenerateSystem();
        }

        private List<GameObject> GenerateDefinedSatellites(SatelliteGenerationParameters parameters, Random random)
        {
            var satellites = new List<GameObject>();

            foreach (var planetAndOrbitData in parameters.Satellites)
            {
                PlanetData planetData = planetAndOrbitData.PlanetData;
                var satelliteName = planetData.name;

                var anchor = Instantiator.InstantiateGameObject(anchorPrefab, parameters.ParentTransform.parent);
                var anchorOrbit = anchor.GetComponent<OrbitalMovement>();
                anchorOrbit.SetParameters(planetAndOrbitData.OrbitParameters);
                
                var prefab = planetData.PrefabOverride ? planetData.PrefabOverride : planetPrefab;

                var satellite = Instantiator.InstantiateGameObject(prefab, anchor.transform);
                satellite.transform.localPosition = Vector3.zero;
                var satelliteGenerator = satellite.GetComponent<PlanetGenerator>();
                satelliteGenerator.SetBodyData(planetData);
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