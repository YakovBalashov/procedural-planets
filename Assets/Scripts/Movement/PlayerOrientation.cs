using UnityEngine;
using UnityEngine.InputSystem;

namespace ProceduralPlanets.Movement
{
    public class PlayerOrientation : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float rotationTime = 0.1f;
        [SerializeField] private InputActionReference pitchAction;
        [SerializeField] private InputActionReference rollAction;
        [SerializeField] private InputActionReference yawAction;

        private Vector3 _currentRotation;
        private Vector3 _targetRotation;
        private Vector3 _currentVelocity;

        private void OnEnable()
        {
            pitchAction.action.Enable();
            yawAction.action.Enable();
            rollAction.action.Enable();
        }
        
        private void OnDisable()
        {
            pitchAction.action.Disable();
            yawAction.action.Disable();
            rollAction.action.Disable();
        }
        
        private void Update()
        {
            var pitchInput = pitchAction.action.ReadValue<float>();
            var yawInput = yawAction.action.ReadValue<float>();
            var rollInput = rollAction.action.ReadValue<float>();

            _targetRotation = new Vector3(pitchInput, yawInput, rollInput) * rotationSpeed;
            _currentRotation = Vector3.SmoothDamp(_currentRotation, _targetRotation, ref _currentVelocity,  rotationTime);
            transform.Rotate(_currentRotation * Time.deltaTime, Space.Self);
        }
    }
}
