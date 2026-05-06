using System;
using System.Collections.Generic;
using System.Globalization;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Movement;
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
        [SerializeField] private GameObject moonButtonPrefab;

        [SerializeField] private RectTransform buttonContainer;
        [SerializeField] private GameObject mapPanel;

        [SerializeField] private InputActionReference toggleMapAction;

        [SerializeField] private PlayerInverseMovement playerMovement;

        public event Action<Vector2> OnBodySelected;

        private void OnEnable()
        {
            toggleMapAction.action.Enable();
            toggleMapAction.action.performed += ToggleMap;
            playerMovement.OnMovementStarted += TurnOffMap;
        }

        private void OnDisable()
        {
            toggleMapAction.action.performed -= ToggleMap;
            toggleMapAction.action.Disable();
            playerMovement.OnMovementStarted -= TurnOffMap;
        }

        private void TurnOffMap()
        {
            mapPanel.SetActive(false);
        }

        private void ToggleMap(InputAction.CallbackContext obj)
        {
            mapPanel.SetActive(!mapPanel.activeSelf);
        }

        public void Generate(List<GameObject> celestialBodies)
        {
            var starColor = celestialBodies[0].GetComponent<CelestialBodyGeneratorBase>().GetBodyData().BaseColor;
            GenerateButton(starButtonPrefab, celestialBodies[0].name, starColor, Vector2.zero, buttonContainer);

            var bodyIndex = new Vector2(1, 0);

            foreach (var body in celestialBodies)
            {
                var bodyData = body.GetComponent<CelestialBodyGeneratorBase>().GetBodyData();
                if (bodyData is StarData) continue;
                bodyIndex.y = 0;

                var bodyName = bodyIndex.x.ToString(CultureInfo.InvariantCulture);
                var bodyColor = new Color(bodyData.BaseColor.r, bodyData.BaseColor.g, bodyData.BaseColor.b, 1f);

                var planetButton = GenerateButton(planetButtonPrefab, bodyName, bodyColor, bodyIndex, buttonContainer);

                GenerateMoonButtons(body, (int)bodyIndex.x, planetButton.transform.GetChild(1));

                bodyIndex.x += 1;
            }
        }

        private void GenerateMoonButtons(GameObject planet, int planetIndex, Transform parent)
        {
            var planetGenerator = planet.GetComponent<PlanetGenerator>();
            if (!planetGenerator) return;

            if (planetGenerator.Moons.Count == 0) return;

            var moonIndex = 1;
            foreach (var moon in planetGenerator.Moons)
            {
                var moonButtonIndex = new Vector2(planetIndex, moonIndex);

                var moonName = $"{SystemGenerator.NumberToLetter(moonIndex - 1)}";
                var moonColor = moon.GetComponent<CelestialBodyGeneratorBase>().GetBodyData().BaseColor;
                moonColor.a = 1f;

                GenerateButton(moonButtonPrefab, moonName, moonColor, moonButtonIndex, parent);
                moonIndex++;
            }
        }

        private GameObject GenerateButton(GameObject prefab, string bodyName, Color color, Vector2 bodyIndex,
            Transform parent)
        {
            GameObject buttonObject = Instantiate(prefab, parent);
            Button button = buttonObject.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = bodyName;
            button.image.color = color;

            button.onClick.AddListener(() => SelectBody(bodyIndex));
            return buttonObject;
        }

        private void SelectBody(Vector2 bodyIndex)
        {
            OnBodySelected?.Invoke(bodyIndex);
        }
    }
}