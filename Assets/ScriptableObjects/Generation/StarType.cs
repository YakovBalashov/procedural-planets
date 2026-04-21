using System.Collections.Generic;
using ProceduralPlanets.Generation;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "StarType", menuName = "Planetary Systems/Celestial Body Types/Star Type")]
    public class StarType : CelestialBodyType<StarData>
    {
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