using System.Collections;
using System.Collections.Generic;
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

        private Vector3 _targetPosition;
        private bool _hasTarget;
        private int _trackingSign;

        private bool _isAlignmentInProgress;
        private bool _isPlayerInputEnabled = true;

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
            if (!_isPlayerInputEnabled) return;
            HandlePlayerInput();
        }
        
        public void TogglePlayerInput(bool isEnabled)
        {
            _isPlayerInputEnabled = isEnabled;
        }

        private void HandlePlayerInput()
        {
            var pitchInput = pitchAction.action.ReadValue<float>();
            var yawInput = yawAction.action.ReadValue<float>();
            var rollInput = rollAction.action.ReadValue<float>();

            _targetRotation = new Vector3(pitchInput, yawInput, rollInput) * rotationSpeed;
            _currentRotation =
                Vector3.SmoothDamp(_currentRotation, _targetRotation, ref _currentVelocity, rotationTime);
            transform.Rotate(_currentRotation * Time.deltaTime, Space.Self);
        }

        public void AlignToTargetInTime(Vector3 targetPosition, float durationInSeconds, bool inverseTracking)
        {
            if (_isAlignmentInProgress) return;
            StartCoroutine(SmoothAlignRoutine(targetPosition, durationInSeconds, inverseTracking));
        }

        private IEnumerator SmoothAlignRoutine(Vector3 targetPosition, float duration, bool inverseTracking)
        {
            _isAlignmentInProgress = true;
            Quaternion startRotation = transform.rotation;
            float elapsedTime = 0f;
            Vector3 initialUp = transform.up;

            int trackingSign = inverseTracking ? -1 : 1;
            Vector3 directionToTarget = (transform.position - targetPosition) * trackingSign;

            while (elapsedTime < duration)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget, initialUp);

                float t = elapsedTime / duration;
                float smoothT = Mathf.SmoothStep(0f, 1f, t);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.rotation = Quaternion.LookRotation(directionToTarget, transform.up);

            _isAlignmentInProgress = false;
        }
    }
}