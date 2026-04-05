using System;
using System.Collections.Generic;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProceduralPlanets.UI
{
    public class SystemMap : MonoBehaviour
    {
        [SerializeField] private GameObject starButtonPrefab;
        [SerializeField] private GameObject planetButtonPrefab;
        
        [SerializeField] private RectTransform buttonContainer;
        [SerializeField] private GameObject mapPanel;
        
        [SerializeField] private InputActionReference toggleMapAction;

        public event Action<Vector2> OnBodySelected;

        private void OnEnable()
        {
            toggleMapAction.action.Enable();
            toggleMapAction.action.performed += ToggleMap;
        }
        
        private void OnDisable()
        {
            toggleMapAction.action.performed -= ToggleMap;
            toggleMapAction.action.Disable();
        }

        private void ToggleMap(InputAction.CallbackContext obj)
        {
            mapPanel.SetActive(!mapPanel.activeSelf);
        }

        public void Generate(List<CelestialBodyData> celestialBodies)
        {
            GenerateButton(starButtonPrefab, celestialBodies[0], Vector2.zero);
            
            var bodyIndex = new Vector2(1, 0);
            foreach (var body in celestialBodies)
            {
                if (body is StarData) continue;
                GenerateButton(planetButtonPrefab, body, bodyIndex);
                bodyIndex.x += 1;
            }
        }
        
        private void GenerateButton(GameObject prefab, CelestialBodyData bodyData, Vector2 bodyIndex)
        {
            GameObject buttonObj = Instantiate(prefab, buttonContainer);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = bodyData.name;
            
            button.onClick.AddListener(() => SelectBody(bodyIndex));
        }

        private void SelectBody(Vector2 bodyIndex)
        {
            OnBodySelected?.Invoke(bodyIndex);
        }
    }
}