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

        [Header("Smoothness Settings")]
        [SerializeField] private float speedChangeRate = 10f;

        [SerializeField] private float rotationChangeRate = 45f;
        [SerializeField] private float radiusChangeRate = 0.2f;

        private OrbitalMovement _orbitalMovement;
        private PlayerInverseMovement _playerInverseMovement;
        private float _targetBodyRadius;

        private void Awake()
        {
            _orbitalMovement = GetComponent<OrbitalMovement>();
            _playerInverseMovement = GetComponent<PlayerInverseMovement>();
        }

        private void SetTargetBodyRadius(CelestialBodyData data)
        {
            _targetBodyRadius = data.Radius;
        }

        private void OnEnable()
        {
            changeSpeedAction.action.Enable();
            changeRotationAction.action.Enable();
            changeRadiusAction.action.Enable();

            _playerInverseMovement.OnArrivedAtBody += SetTargetBodyRadius;
        }

        private void OnDisable()
        {
            changeSpeedAction.action.Disable();
            changeRotationAction.action.Disable();
            changeRadiusAction.action.Disable();

            _playerInverseMovement.OnArrivedAtBody -= SetTargetBodyRadius;
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