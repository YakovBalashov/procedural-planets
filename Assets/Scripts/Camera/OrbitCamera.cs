using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProceduralPlanets.Camera
{
    public class OrbitalCameraActionManager : MonoBehaviour
    {
        [SerializeField] private InputActionReference toggleOrbitAction;
        [SerializeField] private InputActionReference lookAction;
        [SerializeField] private InputActionReference zoomAction;

        private void OnEnable()
        {
            toggleOrbitAction.action.Enable();
            zoomAction.action.Enable();
            toggleOrbitAction.action.performed += EnableOrbit;
            toggleOrbitAction.action.canceled += DisableOrbit;
        }

        private void OnDisable()
        {
            toggleOrbitAction.action.Disable();
            zoomAction.action.Disable();
            lookAction.action.Disable();
            toggleOrbitAction.action.performed -= EnableOrbit;
            toggleOrbitAction.action.canceled -= DisableOrbit;
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