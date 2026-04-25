using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProceduralPlanets.Camera
{
    [RequireComponent(typeof(CinemachineCamera))]
    public class OrbitalCameraActionManager : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;

        [SerializeField] private InputActionReference toggleOrbitAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference zoomAction;

        [SerializeField] private float minScale = 0.1f;
        [SerializeField] private float maxScale = 5.0f;
        [SerializeField] private float zoomSensitivity = 0.01f;

        private const float MinScroll = 0.001f;

        private void OnEnable()
        {
            toggleOrbitAction.action.Enable();
            zoomAction.action.Enable();
            toggleOrbitAction.action.performed += EnableOrbit;
            toggleOrbitAction.action.canceled += DisableOrbit;
            zoomAction.action.performed += ScalePlayer;
        }

        private void OnDisable()
        {
            toggleOrbitAction.action.Disable();
            zoomAction.action.Disable();
            lookAction.action.Disable();
            toggleOrbitAction.action.performed -= EnableOrbit;
            toggleOrbitAction.action.canceled -= DisableOrbit;
            zoomAction.action.performed -= ScalePlayer;
        }

        private void ScalePlayer(InputAction.CallbackContext context)
        {
            float scrollValue = context.ReadValue<Vector2>().y;

            if (Math.Abs(scrollValue) < MinScroll) return;

            float currentScale = playerTransform.localScale.x;
            float newScale = Mathf.Clamp(currentScale + scrollValue * zoomSensitivity, minScale, maxScale);
            playerTransform.localScale = new Vector3(newScale, newScale, newScale);
        }

        private void EnableOrbit(InputAction.CallbackContext context)
        {
            lookAction.action.Enable();
        }

        private void DisableOrbit(InputAction.CallbackContext obj)
        {
            lookAction.action.Disable();
        }
    }
}