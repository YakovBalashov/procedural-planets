using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProceduralPlanets.Generation
{
    public class LoadingManager : MonoBehaviour
    {
        private static LoadingManager _instance;
        public static event Action OnLoadingStarted; 
        private void Awake()
        {
            if (_instance is not null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
        }
        
        public void LoadScene(int buildIndex)
        {
            StartCoroutine(PerformLoadingRoutine(buildIndex));
        }

        private static IEnumerator PerformLoadingRoutine(int buildIndex)
        {
            OnLoadingStarted?.Invoke();
            var asyncLoad = SceneManager.LoadSceneAsync(buildIndex);

            while (asyncLoad is { isDone: false })
            {
                yield return null;
            }
        }
    }
}