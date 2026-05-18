using ProceduralPlanets.Generation;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.CelestialBodies
{
    [CreateAssetMenu(fileName = "PlanetSurfaceData", menuName = "Planetary Systems/Celestial Body Data/Planet Data")]
    public class PlanetData : CelestialBodyData
    {
        [field: SerializeField] public CraterGenerationSettings CraterGenerationSettings { get; private set; }
        [field: SerializeField] public RingParameters RingParameters { get; private set; }
        [field: SerializeField] public AtmosphereParameters AtmosphereParameters { get; private set; }
        [field: SerializeField] public GameObject PrefabOverride { get; private set; }

        public void InitializePlanet(CraterGenerationSettings craterGenerationSettings, RingParameters ringParameters,
            AtmosphereParameters atmosphereParameters)
        {
            CraterGenerationSettings = craterGenerationSettings;
            RingParameters = ringParameters;
            AtmosphereParameters = atmosphereParameters;
        }
    }
}