using ProceduralPlanets.Generation;
using UnityEditor;
using UnityEngine;

namespace ProceduralPlanets.CustomInspector
{
    [CustomEditor(typeof(SystemGenerator))]
    public class SystemInspector : Editor
    {
        private SystemGenerator _systemGenerator;

        private void OnEnable()
        {
            _systemGenerator = (SystemGenerator)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();
            
            if (GUILayout.Button("Generate System"))
            {
                _systemGenerator.GenerateSystem();
            }
        }
    }
}