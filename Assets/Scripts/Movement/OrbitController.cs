using System;
using ProceduralPlanets;
using UnityEngine;
using UnityEngine.InputSystem;


namespace ProceduralPlanets.Movement
{
    [RequireComponent(typeof(OrbitalMovement))]
    public class OrbitController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference changeSpeedAction;
        [SerializeField] private InputActionReference changeRotationAction;
        [SerializeField] private InputActionReference changeRadiusAction;
        
        [Header("Smoothness Settings")]
        public float speedChangeRate = 10f;
        public float rotationChangeRate = 45f;
        public float radiusChangeRate = 10f;

        private OrbitalMovement _orbitalMovement;

        private void Awake()
        {
            _orbitalMovement = GetComponent<OrbitalMovement>();
        }

        private void OnEnable()
        {
            changeSpeedAction.action.Enable();
            changeRotationAction.action.Enable();
            changeRadiusAction.action.Enable();
        }
        
        private void OnDisable()
        {
            changeSpeedAction.action.Disable();
            changeRotationAction.action.Disable();
            changeRadiusAction.action.Disable();
        }

        private void Update()
        {
            var speedInput = changeSpeedAction.action.ReadValue<float>();
            var rotationInput = changeRotationAction.action.ReadValue<float>();
            var radiusInput = changeRadiusAction.action.ReadValue<float>();

            if (speedInput != 0)
            {
                float deltaSpeed = speedInput * speedChangeRate * Time.deltaTime;
                _orbitalMovement.ChangeSpeed(deltaSpeed);
            }

            if (rotationInput != 0)
            {
                float deltaRotation = rotationInput * rotationChangeRate * Time.deltaTime;
                _orbitalMovement.ChangeRotation(deltaRotation);
            }

            if (radiusInput != 0)
            {
                float deltaRadius = radiusInput * radiusChangeRate * Time.deltaTime;
                _orbitalMovement.ChangeRadius(deltaRadius);
            }
        }
    }
}