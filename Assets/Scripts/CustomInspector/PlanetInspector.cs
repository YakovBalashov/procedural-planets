using ProceduralPlanets.Generation;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEditor;

namespace ProceduralPlanets.CustomInspector
{
    [CustomEditor(typeof(PlanetGenerator))]
    public class PlanetInspector : Editor
    {
        private PlanetGenerator _planetGenerator;
        private Editor _surfaceEditor;
        
        private void OnEnable()
        {
            _planetGenerator = (PlanetGenerator)target;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            
            base.OnInspectorGUI();
            
            EditorGUILayout.InspectorTitlebar(true, _planetGenerator);
            RenderPlanetSurfaceInspector(_planetGenerator.bodyData);
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_planetGenerator);
                _planetGenerator.UpdateSurface();
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void RenderPlanetSurfaceInspector(PlanetData data)
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
