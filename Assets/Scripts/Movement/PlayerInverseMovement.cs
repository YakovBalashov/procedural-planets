using System;
using System.Collections;
using ProceduralPlanets.Generation;
using ProceduralPlanets.ScriptableObjects.CelestialBodies;
using ProceduralPlanets.UI;
using Unity.Cinemachine;
using UnityEngine;

namespace ProceduralPlanets.Movement
{
    [RequireComponent(typeof(PlayerOrbitalMovement), typeof(PlayerOrientation))]
    public class PlayerInverseMovement : MonoBehaviour
    {
        private const float RadiusCollisionMultiplayer = 1.5f;

        public event Action OnMovementStarted;
        public event Action OnCenter;
        public event Action<CelestialBodyData> OnArrivedAtBody;

        [Header("Movement Settings")]
        [SerializeField] private float planetaryTravelTime = 20f;

        [SerializeField] private float moonTravelTime = 10f;
        [SerializeField] private float initialRotationTime = 3f;
        [SerializeField] private float midflightRotationTime = 1.5f;
        [SerializeField] private float gapTime = 0.5f;
        [field: SerializeField] public float DefaultOrbitalVelocity { get; private set; } = 0.5f;
        [SerializeField] private AnimationCurve planetaryTravelCurve;
        [SerializeField] private bool collisionCheck;

        [Header("References")]
        [SerializeField] private SystemMap systemMap;

        [SerializeField] private SystemGenerator systemGenerator;
        [SerializeField] private Transform systemOrigin;
        [SerializeField] private GameObject engines;

        [Header("Recentering Settings")]
        [SerializeField] private float maxDistanceFromOrigin = 10000f;

        [SerializeField] private CinemachineCamera orbitalCamera;

        private PlayerOrbitalMovement _orbitalMovement;
        private PlayerOrientation _playerOrientation;

        private Vector2 _currentBodyIndex;

        private bool _isMoving;
        private Vector3 _targetPosition;
        private Transform _targetBodyTransform;
        private string _targetBodyName;

        private void Awake()
        {
            _orbitalMovement = GetComponent<PlayerOrbitalMovement>();
            _playerOrientation = GetComponent<PlayerOrientation>();
        }

        private void Start()
        {
            var star = systemGenerator.GetBodyByIndex(Vector2.zero);
            transform.SetParent(star.transform.parent);
            var newOrbit = new OrbitParameters(star.GetBodyData().PlayerOrbitRadius, 1f, 0f, 0f, DefaultOrbitalVelocity);
            _orbitalMovement.SetParameters(newOrbit);
            _orbitalMovement.enabled = true;
            _currentBodyIndex = Vector2.zero;
            OnArrivedAtBody?.Invoke(star.GetBodyData());
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
            if (_isMoving)
            {
                SystemGenerator.MessageText.SetMessage("Already traveling to a destination.",
                    2, MessageText.ErrorColor);
                return;
            }
            
            if (SystemGenerator.CurrentTimeExponent != 0)
            {
                SystemGenerator.MessageText.SetMessage("Set time scale to 1x before traveling to a new destination.",
                    2, MessageText.ErrorColor);
                return;
            }
            
            if (targetBodyIndex == _currentBodyIndex)
            {
                SystemGenerator.MessageText.SetMessage("Already at the selected destination.",
                    2, MessageText.ErrorColor);
                return;
            }

            if (targetBodyIndex.y != 0 && (int)_currentBodyIndex.x != (int)targetBodyIndex.x)
            {
                SystemGenerator.MessageText.SetMessage("Travel to the planet first before trying to reach its moons.",
                    2, MessageText.ErrorColor);
                return;
            }

            var travelTime = (int)targetBodyIndex.x == (int)_currentBodyIndex.x ? moonTravelTime : planetaryTravelTime;

            var currentAnchor = transform.parent;

            _orbitalMovement.enabled = false;
            CenterPlayer();
            transform.SetParent(null);

            var targetBody = systemGenerator.GetBodyByIndex(targetBodyIndex);
            _targetPosition = CalculateTargetPosition(targetBody, travelTime + initialRotationTime + gapTime);
            _targetBodyTransform = targetBody.transform;

            var inverseTravelDirection = (transform.position - _targetPosition).normalized;
            var distanceToTarget = Vector3.Distance(transform.position, _targetPosition);

            var inverseTarget = systemOrigin.position + inverseTravelDirection * distanceToTarget;

            if (collisionCheck && !IsPathCollisionFree())
            {
                transform.SetParent(currentAnchor);
                _orbitalMovement.enabled = true;
                SystemGenerator.MessageText.SetMessage("Path is obstructed! Try changing your orbit", 2,
                    MessageText.ErrorColor);
                return;
            }

            _targetBodyName = targetBody.name;
            SystemGenerator.MessageText.SetMessage("Traveling to " + _targetBodyName, 3, MessageText.InfoColor);

            _isMoving = true;

            StartCoroutine(ExecuteFlight(systemOrigin.position, inverseTarget, travelTime,
                targetBody.GetBodyData(), _targetPosition));

            _currentBodyIndex = targetBodyIndex;
        }

        private bool IsPathCollisionFree()
        {
            var currentBody = systemGenerator.GetBodyByIndex(_currentBodyIndex);
            var currentBodyRadius = currentBody.GetBodyData().Radius;
            var currentBodyPosition = currentBody.transform.position;

            var playerPosition = transform.position;
            var targetPosition = _targetPosition;

            Vector3 pathDirection = targetPosition - playerPosition;
            float pathLength = pathDirection.magnitude;
            pathDirection.Normalize();

            Vector3 playerToCenter = currentBodyPosition - playerPosition;

            float centerProjection = Vector3.Dot(playerToCenter, pathDirection);

            if (centerProjection < 0f) return true;

            float distanceToCenterSquared = playerToCenter.sqrMagnitude - (centerProjection * centerProjection);

            float safeRadius = currentBodyRadius * RadiusCollisionMultiplayer;
            float radiusSquared = safeRadius * safeRadius;

            if (distanceToCenterSquared > radiusSquared) return true;

            float surfaceHitDistance = centerProjection - Mathf.Sqrt(radiusSquared - distanceToCenterSquared);

            if (surfaceHitDistance > pathLength) return true;

            return false;
        }

        private IEnumerator ExecuteFlight(Vector3 startPos, Vector3 targetPos, float travelTime, CelestialBodyData data,
            Vector3 trackingPosition)
        {
            OnMovementStarted?.Invoke();

            yield return RotateShip(trackingPosition);

            var elapsedTime = 0f;
            float halfwayTime = travelTime / 2f;
            bool midflightRotationDone = false;

            while (elapsedTime < travelTime)
            {
                elapsedTime += Time.deltaTime;

                if (!midflightRotationDone && elapsedTime >= halfwayTime)
                {
                    StartCoroutine(PerformMidflightRotation(trackingPosition));
                    midflightRotationDone = true;
                }

                var t = planetaryTravelCurve.Evaluate(elapsedTime / travelTime);
                systemOrigin.position = Vector3.Lerp(startPos, targetPos, t);

                yield return null;
            }

            systemOrigin.position = targetPos;
            engines.SetActive(false);
            _isMoving = false;

            _playerOrientation.TogglePlayerInput(true);
            OrbitTargetBody(DefaultOrbitalVelocity, _targetBodyTransform.parent);
            SystemGenerator.MessageText.SetMessage("Arrived at " + _targetBodyName, 3, MessageText.SuccessColor);
            OnArrivedAtBody?.Invoke(data);
        }

        private IEnumerator RotateShip(Vector3 trackingPosition)
        {
            _playerOrientation.TogglePlayerInput(false);
            _playerOrientation.AlignToTargetInTime(trackingPosition, initialRotationTime, false);
            yield return new WaitForSeconds(initialRotationTime + gapTime);
            engines.SetActive(true);
        }

        private IEnumerator PerformMidflightRotation(Vector3 trackingPosition)
        {
            engines.SetActive(false);
            yield return new WaitForSeconds(gapTime);

            _playerOrientation.AlignToTargetInTime(trackingPosition, midflightRotationTime, true);
            yield return new WaitForSeconds(midflightRotationTime + gapTime);
            engines.SetActive(true);
        }

        private void OrbitTargetBody(float orbitalVelocity, Transform anchor)
        {
            transform.SetParent(anchor);
            _orbitalMovement.SetCircularOrbitFromCurrentPosition(orbitalVelocity);
            _orbitalMovement.enabled = true;
        }

        private Vector3 CalculateTargetPosition(CelestialBodyGeneratorBase targetBody, float time)
        {
            var targetBodyPosition = CalculateFutureTargetBodyPosition(targetBody, time);

            var playerPosition = transform.position;
            var directionFromTarget = (playerPosition - targetBodyPosition).normalized;
            var offsetDistance = targetBody.GetBodyData().PlayerOrbitRadius;
            return targetBodyPosition + directionFromTarget * offsetDistance;
        }

        private Vector3 CalculateFutureTargetBodyPosition(CelestialBodyGeneratorBase targetBody, float time)
        {
            var targetOrbit = targetBody.gameObject.GetComponentInParent<OrbitalMovement>();

            if (!targetOrbit) return targetBody.transform.position;

            var localPosition = targetOrbit.GetPositionAfterTime(time);

            var parentOrbit = targetOrbit.transform.parent.GetComponentInParent<OrbitalMovement>();

            if (!parentOrbit) return targetOrbit.transform.parent.TransformPoint(localPosition);

            var parentPosition = parentOrbit.GetPositionAfterTime(time);
            return parentOrbit.transform.parent.TransformPoint(parentPosition) + localPosition;
        }

        private void CenterPlayer()
        {
            var globalPosition = transform.position;

            systemOrigin.SetParent(transform, true);

            transform.position = Vector3.zero;
            systemOrigin.SetParent(null);

            var deltaPosition = transform.position - globalPosition;
            orbitalCamera?.OnTargetObjectWarped(transform, deltaPosition);
            OnCenter?.Invoke();
        }
        
        private void OnDrawGizmos()
        {
            if (!_isMoving) return;

            Gizmos.color = _orbitalMovement.Color;
            Gizmos.DrawLine(transform.position, _targetBodyTransform.position);
        }
    }
}