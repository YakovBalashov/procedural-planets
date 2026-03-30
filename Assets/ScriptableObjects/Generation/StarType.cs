using System.Collections.Generic;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "StarType", menuName = "Planetary Systems/Celestial Body Types/Star Type")]
    public class StarType : CelestialBodyType<StarData>
    {
        [field: SerializeField] public Vector2 PlanetNumberRange { get; private set; }
        [field: SerializeField] public Vector2 PlanetOrbitRadiusRange { get; private set; }
        [field: SerializeField] public Vector2 DistanceBetweenPlanetsRange { get; private set; }
        
        [field: SerializeField] public List<StarType> CompatibleStarTypes { get; private set; }
        [field: SerializeField] public Vector2 StarNumberRange { get; private set; }
        [field: SerializeField] public Material StarMaterial { get; private set; }

        public override StarData CreateInstance(int seed)
        {
            StarData instance = base.CreateInstance(seed);
            instance.SetMaterial(StarMaterial);
            return instance;
        }
    }
}
