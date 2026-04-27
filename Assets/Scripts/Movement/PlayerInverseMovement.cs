using System.Collections;
using ProceduralPlanets.Generation;
using ProceduralPlanets.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace ProceduralPlanets.Movement
{
    [RequireComponent(typeof(OrbitalMovement))]
    public class PlayerInverseMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float planetaryTravelTime = 20f;

        [SerializeField] private float moonTravelTime = 10f;
        [SerializeField] private float flightSteepness = 2f;

        [Header("System References")]
        [SerializeField] private SystemMap systemMap;

        [SerializeField] private SystemGenerator systemGenerator;
        [SerializeField] private Transform systemOrigin;

        [Header("Recentering Settings")]
        [SerializeField] private float maxDistanceFromOrigin = 10000f;

        [SerializeField] private CinemachineCamera orbitalCamera;
        [SerializeField] private AnimationCurve planetaryTravelCurve;

        private OrbitalMovement _orbitalMovement;

        private bool _isMoving;
        private Vector3 _targetPosition;
        private Transform _targetBodyTransform;

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

        private void LateUpdate()
        {
            if (_isMoving) return;
            if (!(transform.position.magnitude > maxDistanceFromOrigin)) return;
            CenterPlayer();
        }

        private void MoveToBody(Vector2 targetBodyIndex)
        {
            _orbitalMovement.enabled = false;
            CenterPlayer();
            transform.SetParent(null);

            var targetBody = systemGenerator.GetBodyByIndex(targetBodyIndex);
            _targetPosition = CalculateTargetPosition(targetBody);
            _targetBodyTransform = targetBody.transform;
            _isMoving = true;

            var inverseTravelDirection = (transform.position - _targetPosition).normalized;
            var distanceToTarget = Vector3.Distance(transform.position, _targetPosition);

            var inverseTarget = systemOrigin.position + inverseTravelDirection * distanceToTarget;

            StartCoroutine(ExecuteFlight(systemOrigin.position, inverseTarget, planetaryTravelTime,
                targetBody.GetBodyData().PlayerOrbitRadius));
        }

        private IEnumerator ExecuteFlight(Vector3 startPos, Vector3 targetPos, float travelTime, float radius)
        {
            var elapsedTime = 0f;

            while (elapsedTime < travelTime)
            {
                elapsedTime += Time.deltaTime;
                var t = planetaryTravelCurve.Evaluate(elapsedTime / travelTime);
                systemOrigin.position = Vector3.Lerp(startPos, targetPos, t);

                yield return null;
            }

            systemOrigin.position = targetPos;
            _isMoving = false;

            OrbitTargetBody(radius);
        }

        private void OrbitTargetBody(float radius)
        {
            transform.SetParent(_targetBodyTransform.parent);
            var newOrbit = new OrbitParameters(radius, 1f, 0f, 0f, 5);
            _orbitalMovement.SetParameters(newOrbit);
            _orbitalMovement.CalculateAngleFromPositionForPerfectOrbit();
            _orbitalMovement.enabled = true;
        }

        private Vector3 CalculateTargetPosition(CelestialBodyGeneratorBase targetBody)
        {
            var targetOrbit = targetBody.gameObject.GetComponentInParent<OrbitalMovement>();

            var targetBodyPosition = targetOrbit
                ? targetOrbit.GetPositionAfterTime(planetaryTravelTime)
                : targetBody.transform.position;

            var playerPosition = transform.position;
            var directionFromTarget = (playerPosition - targetBodyPosition).normalized;
            var offsetDistance = targetBody.GetBodyData().PlayerOrbitRadius;
            return targetBodyPosition + directionFromTarget * offsetDistance;
        }

        private void CenterPlayer()
        {
            var parent = transform.parent;
            var localPosition = transform.localPosition;
            var globalPosition = transform.position;

            transform.SetParent(null);
            systemOrigin.SetParent(transform, true);

            transform.position = Vector3.zero;
            systemOrigin.SetParent(null);

            transform.parent = parent;
            transform.localPosition = localPosition;

            var deltaPosition = transform.position - globalPosition;
            orbitalCamera?.OnTargetObjectWarped(transform, deltaPosition);
        }

        private void OnDrawGizmos()
        {
            if (!_isMoving) return;

            Gizmos.DrawLine(transform.position, _targetBodyTransform.position);
        }
    }
}