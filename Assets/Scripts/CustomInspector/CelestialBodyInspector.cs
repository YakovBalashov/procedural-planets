using ProceduralPlanets.Generation;
using UnityEditor;
using UnityEngine;

namespace ProceduralPlanets.CustomInspector
{
    [CustomEditor(typeof(CelestialBodyGeneratorBase), true)]
    public class CelestialBodyInspector : Editor
    {
        private CelestialBodyGeneratorBase _bodyGenerator;
        private Editor _surfaceEditor;
        
        private void OnEnable()
        {
            _bodyGenerator = (CelestialBodyGeneratorBase)target;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Generate Body Data")) 
            {
                _bodyGenerator.GenerateBodyData();
            }
            
            EditorGUILayout.InspectorTitlebar(true, _bodyGenerator);
            RenderPlanetSurfaceInspector(_bodyGenerator.GetBodyData());
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_bodyGenerator);
                _bodyGenerator.UpdateSurface();
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void RenderPlanetSurfaceInspector(CelestialBodyData data)
        {
            if (!data)
            {
                EditorGUILayout.HelpBox("Assign a PlanetSurfaceData to edit surface properties.", MessageType.Warning);
                return;
            }
            CreateCachedEditor(data, null, ref _surfaceEditor);
            _surfaceEditor.OnInspectorGUI();
        }
    }
}
