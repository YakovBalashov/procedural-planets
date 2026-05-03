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

        protected override Mesh GenerateMeshOnGPU(int sphereResolution)
        {
            var craters = CraterGenerator.GenerateCraters(BodyData.CraterGenerationSettings, 0);
            using var craterBuffer =
                new ComputeBuffer(Mathf.Max(1, craters.Count), Marshal.SizeOf(typeof(CraterParameters)));
            if (craters.Count > 0) craterBuffer.SetData(craters);
            int geometryKernel = displacementShader.FindKernel("CSGeometry");
            displacementShader.SetBuffer(geometryKernel, ShaderParametersIDs.CraterParameters, craterBuffer);
            displacementShader.SetInt(ShaderParametersIDs.CraterCount, craters.Count);
            
            return base.GenerateMeshOnGPU(sphereResolution);
        }

        public override void UpdateSurface()
        {
            base.UpdateSurface();

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