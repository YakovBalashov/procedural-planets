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
            CelestialBodyGeneratorBase targetBody = systemGenerator.GetBodyByIndex(targetBodyIndex);
            
            if (_isMoving) return;
            
            _orbitalMovement.enabled = false;

            transform.SetParent(targetBody.transform, true);
            _targetBodyTransform = targetBody.transform;
            _targetPositionLocal = CalculateTargetPosition(targetBody);

            _isMoving = true;
            StartCoroutine(Move(targetBody.GetBodyData().Radius * orbitToBodyRadiusRatio));
        }

        private Vector3 CalculateTargetPosition(CelestialBodyGeneratorBase targetBody)
        {
            var bodyRadius = targetBody.GetBodyData().Radius;
            
            var direction = Vector3.ProjectOnPlane(transform.localPosition.normalized, targetBody.transform.up); 
            
            var targetPosition = direction * bodyRadius * orbitToBodyRadiusRatio;
            return targetPosition;
        }

        private IEnumerator Move(float radius)
        {
            var currentDistance = Vector3.Distance(transform.localPosition, _targetPositionLocal);
            var decelerationDistance = radius * decelerationDistanceToRadiusRatio;
            
            while (currentDistance > _distanceLimit)
            {
                currentDistance = Vector3.Distance(transform.localPosition, _targetPositionLocal);
                
                var currentVelocity = velocity;
                
                if (currentDistance < decelerationDistance)
                {
                    var t = currentDistance / decelerationDistance;
                    t = Mathf.SmoothStep(0f, 1f, t);
                    currentVelocity = Mathf.Lerp(minVelocity, velocity, t);
                }
                
                var step = currentVelocity * Time.deltaTime;
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetPositionLocal, step);
                yield return null;
            }
            transform.localPosition = _targetPositionLocal;
            _isMoving = false;
            var newOrbit = new OrbitParameters(radius, 1f, 0f, 0f, orbitalVelocity);
            
            _orbitalMovement.SetParameters(newOrbit);
            _orbitalMovement.CalculateAngleFromPositionForPerfectOrbit();
            _orbitalMovement.enabled = true;
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
