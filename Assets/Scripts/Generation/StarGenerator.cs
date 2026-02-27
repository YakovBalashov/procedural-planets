using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(Light))]
    public class StarGenerator : CelestialBodyGenerator<StarData, StarType>
    {
        private Light _pointLight;
        protected override void Initialize()
        {
            base.Initialize();
            if (_pointLight) return;
            _pointLight = GetComponent<Light>();
        }

        public override void UpdateSurface()
        {
            base.UpdateSurface();
            _pointLight.color = BodyData.SurfaceMaterial.color;
        }
    }
}