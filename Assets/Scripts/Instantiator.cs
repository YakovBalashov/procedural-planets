using UnityEditor;
using UnityEngine;

namespace ProceduralPlanets
{
    public static class Instantiator
    {
        public static GameObject InstantiateGameObject(GameObject prefab, Transform parent)
        {
            if (Application.isPlaying)
            {
                return Object.Instantiate(prefab, parent);
            }

            #if UNITY_EDITOR
            return (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            #else
            return Object.Instantiate(prefab, parent);
            #endif
        }
    }
}