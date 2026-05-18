using System.Collections.Generic;
using System.Linq;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Movement;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;
using ProceduralPlanets.Extensions;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "PlanetType", menuName = "Planetary Systems/Generation Types/Planet Type")]
    public class PlanetType : CelestialBodyType<PlanetData>
    {
        [field: SerializeField] public OrbitType<StarType, StarData> StarOrbitType { get; private set; }
        [field: SerializeField] public OrbitType<PlanetType, PlanetData> PlanetOrbitType { get; private set; }
        [field: SerializeField] public CraterGenerationSettings CraterGenerationSettings { get; private set; }
        [field: SerializeField] public RingType Rings { get; private set; }
        [field: SerializeField] public AtmosphereType Atmosphere { get; private set; }
        [field: SerializeField] public GameObject PrefabOverride { get; private set; }

        public override PlanetData CreateInstance(int seed)
        {
            var random = new System.Random(seed);
            PlanetData instance = base.CreateInstance(seed);
            
            var ringParameters = Rings ? Rings.GenerateRingParameters(random.Next(0, int.MaxValue)) : new RingParameters();
            var atmosphereParameters = Atmosphere ? Atmosphere.GenerateAtmosphereParameters(random.Next(0, int.MaxValue)) : new AtmosphereParameters();

            instance.InitializePlanet(CraterGenerationSettings, ringParameters, atmosphereParameters);

            return instance;
        }
    }
}