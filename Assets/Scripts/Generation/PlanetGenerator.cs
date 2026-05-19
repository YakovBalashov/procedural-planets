using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.Noise;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.ScriptableObjects.Generation;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProceduralPlanets.Generation
{
    public class PlanetGenerator : CelestialBodyGenerator<PlanetData, PlanetType>
    {
        [SerializeField] private GameObject atmospherePrefab;
        [SerializeField] private GameObject ringsPrefab;

        public List<GameObject> Moons { get; set; }

        private PlanetaryRingsGenerator _ringsGenerator;
        private AtmosphereGenerator _atmosphereGenerator;

        protected override void Initialize()
        {
            base.Initialize();

            if (BodyData.AtmosphereParameters.Enabled && !_atmosphereGenerator)
            {
                _atmosphereGenerator = Instantiator.InstantiateGameObject(atmospherePrefab, transform)
                    .GetComponent<AtmosphereGenerator>();
            }

            if (BodyData.RingParameters.Enabled && !_ringsGenerator)
            {
                _ringsGenerator = Instantiator.InstantiateGameObject(ringsPrefab, transform)
                    .GetComponent<PlanetaryRingsGenerator>();
            }
        }

        protected override ComputeBuffer CreateCraterBuffer()
        {
            var craters = CraterGenerator.GenerateCraters(BodyData.CraterGenerationSettings, 0);
            
            if (craters.Count == 0)
                return new ComputeBuffer(1, Marshal.SizeOf(typeof(byte)));
            
            var craterBuffer =
                new ComputeBuffer(Mathf.Max(1, craters.Count), Marshal.SizeOf(typeof(CraterParameters)));
            
            craterBuffer.SetData(craters);
            return craterBuffer;
        }
        
        public override void UpdateSurface(int seed)
        {
            base.UpdateSurface(seed);

            if (BodyData.AtmosphereParameters.Enabled)
                _atmosphereGenerator.UpdateAtmosphere(BodyData.AtmosphereParameters, BodyData.Radius);

            if (BodyData.RingParameters.Enabled) _ringsGenerator.UpdateRings(BodyData.RingParameters);
        }

#if UNITY_EDITOR
        [ContextMenu("Save Planet Data to Asset")]
        public void SavePlanetDataToAsset()
        {
            if (BodyData == null) return;

            if (EditorUtility.IsPersistent(BodyData))
            {
                EditorUtility.SetDirty(BodyData);
                AssetDatabase.SaveAssets();
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Planet Data",
                $"{gameObject.name}",
                "asset",
                "Save procedural planet data"
            );

            if (string.IsNullOrEmpty(path)) return;

            AssetDatabase.CreateAsset(BodyData, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"<color=cyan>Saved Planet Data:</color> {path}");
        }
#endif
    }
}