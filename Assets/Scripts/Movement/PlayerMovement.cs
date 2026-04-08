using System;
using System.Collections;
using ProceduralPlanets.Generation;
using ProceduralPlanets.UI;
using UnityEngine;

namespace ProceduralPlanets.Movement
{
    [RequireComponent(typeof(OrbitalMovement))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float orbitToBodyRadiusRatio = 1.5f;

        [SerializeField] private float velocity = 10f;
        [SerializeField] private float decelerationDistanceToRadiusRatio = 3f;
        [SerializeField] private float minVelocity = 0.1f;
        [SerializeField] private float orbitalVelocity = 2f;

        [SerializeField, Tooltip("Acceleration duration in seconds")]
        private float accelerationDuration = 5f;


        [Header("Gizmos Settings")]
        [SerializeField] private Color trajectoryColor = Color.yellow;

        [SerializeField] private Color destinationColor = Color.orange;
        [SerializeField] private float destinationGizmoRadius = 5f;


        [Header("System References")]
        [SerializeField] private SystemGenerator systemGenerator;

        [SerializeField] private SystemMap systemMap;

        private OrbitalMovement _orbitalMovement;

        private bool _isMoving = false;
        private Vector3 _targetPositionLocal;
        private Transform _targetBodyTransform;

        private float _distanceLimit = 0.001f;

        private void Awake()
        {
            _orbitalMovement = GetComponent<OrbitalMovement>();
        }

        private void OnEnable()
        {
            systemMap.OnBodySelected += MoveToBody;
        }

        private void OnDisable()
        {
            systemMap.OnBodySelected -= MoveToBody;
        }

        private void MoveToBody(Vector2 targetBodyIndex)
        {
            if (_isMoving) return;

            CelestialBodyGeneratorBase targetBody = systemGenerator.GetBodyByIndex(targetBodyIndex);

            _orbitalMovement.enabled = false;

            _targetBodyTransform = targetBody.transform;
            _targetPositionLocal = Vector3.zero;

            _isMoving = true;
            StartCoroutine(ExecuteFlight(targetBody.GetBodyData().Radius * orbitToBodyRadiusRatio));
        }

        private Vector3 CalculateTargetPosition(Transform targetBody, float orbitRadius)
        {
            var positionRelativeToTarget = targetBody.transform.InverseTransformPoint(transform.position);

            var direction = Vector3.ProjectOnPlane(positionRelativeToTarget.normalized, targetBody.transform.up);

            var targetPosition = direction * orbitRadius;
            return targetPosition;
        }

        private IEnumerator ExecuteFlight(float radius)
        {
            var decelerationDistance = radius * decelerationDistanceToRadiusRatio;
            
            yield return StartCoroutine(Accelerate(radius));

            _targetPositionLocal = CalculateTargetPosition(_targetBodyTransform, radius);
            transform.SetParent(_targetBodyTransform, true);

            yield return StartCoroutine(Coast(decelerationDistance));

            yield return StartCoroutine(Decelerate(radius));

            transform.localPosition = _targetPositionLocal;
            _isMoving = false;

            OrbitTargetBody(radius);
        }

        private void OrbitTargetBody(float radius)
        {
            var newOrbit = new OrbitParameters(radius, 1f, 0f, 0f, orbitalVelocity);
            _orbitalMovement.SetParameters(newOrbit);
            _orbitalMovement.CalculateAngleFromPositionForPerfectOrbit();
            _orbitalMovement.enabled = true;
        }

        private IEnumerator Accelerate(float decelerationDistance)
        {
            float currentVelocity = 0f;
            float elapsedTime = 0f;

            while (currentVelocity < velocity)
            {
                var currentDistance = Vector3.Distance(transform.position,
                    _targetBodyTransform.TransformPoint(_targetPositionLocal));
                if (currentDistance <= decelerationDistance) yield break;

                elapsedTime += Time.deltaTime;
                var t = elapsedTime / accelerationDuration;

                currentVelocity = Mathf.SmoothStep(0f, velocity, t);

                MoveOneStepTowardsTargetWorld(currentVelocity);
                yield return null;
            }
        }

        private IEnumerator Coast(float decelerationDistance)
        {
            while (Vector3.Distance(transform.localPosition, _targetPositionLocal) > decelerationDistance)
            {
                MoveOneStepTowardsTargetLocal(velocity);
                yield return null;
            }
        }

        private IEnumerator Decelerate(float radius)
        {
            var currentDistance = Vector3.Distance(transform.localPosition, _targetPositionLocal);
            var decelerationDistance = radius * decelerationDistanceToRadiusRatio;

            while (currentDistance > _distanceLimit)
            {
                currentDistance = Vector3.Distance(transform.localPosition, _targetPositionLocal);

                var t = currentDistance / decelerationDistance;
                t = Mathf.SmoothStep(0f, 1f, t);
                var currentVelocity = Mathf.Lerp(minVelocity, velocity, t);

                MoveOneStepTowardsTargetLocal(currentVelocity);
                yield return null;
            }
        }

        private void MoveOneStepTowardsTargetLocal(float currentVelocity)
        {
            var step = currentVelocity * Time.deltaTime;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetPositionLocal, step);
        }

        private void MoveOneStepTowardsTargetWorld(float currentVelocity)
        {
            var step = currentVelocity * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _targetBodyTransform.position, step);
        }

        private void OnDrawGizmos()
        {
            if (!_isMoving) return;

            Vector3 targetPositionWorld = _targetBodyTransform.TransformPoint(_targetPositionLocal);

            Gizmos.color = trajectoryColor;
            Gizmos.DrawLine(transform.position, targetPositionWorld);
            Gizmos.color = destinationColor;
            Gizmos.DrawSphere(targetPositionWorld, destinationGizmoRadius);
        }
    }
}