using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using UnityEngine;
using UnityEngine.InputSystem;


namespace ProceduralPlanets.Movement
{
    [RequireComponent(typeof(OrbitalMovement), typeof(PlayerInverseMovement))]
    public class OrbitController : MonoBehaviour
    {
        [Header("Input Actions")]
        [SerializeField] private InputActionReference changeSpeedAction;

        [SerializeField] private InputActionReference changeRotationAction;
        [SerializeField] private InputActionReference changeRadiusAction;
        [SerializeField] private InputActionReference zeroSpeedAction;
        [SerializeField] private InputActionReference resetOrbitAction;

        [Header("Smoothness Settings")]
        [SerializeField] private float speedChangeRate = 10f;

        [SerializeField] private float rotationChangeRate = 45f;
        [SerializeField] private float radiusChangeRate = 0.2f;

        private PlayerOrbitalMovement _orbitalMovement;
        private PlayerInverseMovement _playerInverseMovement;
        private float _targetBodyRadius;
        private float _targetBodyOrbitRadius;

        private void Awake()
        {
            _orbitalMovement = GetComponent<PlayerOrbitalMovement>();
            _playerInverseMovement = GetComponent<PlayerInverseMovement>();
        }

        private void SetTargetBodyRadius(CelestialBodyData data)
        {
            _targetBodyRadius = data.Radius;
            _targetBodyOrbitRadius = data.PlayerOrbitRadius;
        }

        private void OnEnable()
        {
            changeSpeedAction.action.Enable();
            changeRotationAction.action.Enable();
            changeRadiusAction.action.Enable();
            resetOrbitAction.action.Enable();
            zeroSpeedAction.action.Enable();

            resetOrbitAction.action.performed += ResetOrbit;
            zeroSpeedAction.action.performed += ZeroSpeed;
            _playerInverseMovement.OnArrivedAtBody += SetTargetBodyRadius;
        }

        private void ResetOrbit(InputAction.CallbackContext obj)
        {
            var parameters = new OrbitParameters(_targetBodyOrbitRadius, 1f, 0, 0,
                _playerInverseMovement.DefaultOrbitalVelocity);
            _orbitalMovement.SetParameters(parameters);
        }

        private void OnDisable()
        {
            changeSpeedAction.action.Disable();
            changeRotationAction.action.Disable();
            changeRadiusAction.action.Disable();
            resetOrbitAction.action.Disable();
            zeroSpeedAction.action.Disable();

            resetOrbitAction.action.performed -= ResetOrbit;
            zeroSpeedAction.action.performed -= ZeroSpeed;

            _playerInverseMovement.OnArrivedAtBody -= SetTargetBodyRadius;
        }

        private void ZeroSpeed(InputAction.CallbackContext obj)
        {
            _orbitalMovement.ZeroSpeed();
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
                _orbitalMovement.ChangeInclination(deltaRotation);
            }

            if (radiusInput != 0)
            {
                float deltaRadius = radiusInput * radiusChangeRate * _targetBodyRadius * Time.deltaTime;
                _orbitalMovement.ChangeRadius(deltaRadius);
            }
        }
    }
}