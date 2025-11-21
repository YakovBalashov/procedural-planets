using ProceduralPlanets.Surface;
using UnityEditor;

namespace ProceduralPlanets.CustomInspector
{
    [CustomEditor(typeof(PlanetSurface))]
    public class PlanetInspector : Editor
    {
        
        private PlanetSurface _planetSurface;
        private Editor _surfaceEditor;
        
        private void OnEnable()
        {
            _planetSurface = (PlanetSurface)target;
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            
            base.OnInspectorGUI();
            
            EditorGUILayout.InspectorTitlebar(true, _planetSurface);
            RenderPlanetSurfaceInspector(_planetSurface.surfaceData);
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_planetSurface);
                _planetSurface.UpdateSurface();
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void RenderPlanetSurfaceInspector(PlanetSurfaceData surfaceData)
        {
            if (!surfaceData)
            {
                EditorGUILayout.HelpBox("Assign a PlanetSurfaceData to edit surface properties.", MessageType.Warning);
                return;
            }
            CreateCachedEditor(surfaceData, null, ref _surfaceEditor);
            _surfaceEditor.OnInspectorGUI();
        }
    }
}
