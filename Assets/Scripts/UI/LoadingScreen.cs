using System;
using ProceduralPlanets.Generation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ProceduralPlanets.UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI loadingText;
        private Image _backgroundImage;

        private void OnEnable()
        {
            LoadingManager.OnLoadingStarted += TurnOn;
        }
        
        private void OnDisable()
        {
            LoadingManager.OnLoadingStarted -= TurnOn;
        }

        private void Awake()
        {
            _backgroundImage = GetComponent<Image>();
            _backgroundImage.enabled = false;
            loadingText.enabled = false;
        }

        private void TurnOn()
        {
            _backgroundImage.enabled = true;
            loadingText.enabled = true;
        }
    }
}
