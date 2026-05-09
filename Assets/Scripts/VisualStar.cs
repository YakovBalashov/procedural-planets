using System;
using ProceduralPlanets.BaseMesh;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Movement;
using UnityEngine;

namespace ProceduralPlanets
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class VisualStar : MeshGenerator
    {
        [SerializeField] private SystemGenerator systemGenerator;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerInverseMovement playerMovement;
        [SerializeField] private int resolution = 20;
        [SerializeField] private float radius = 10f;
        [SerializeField] private float distanceFromPlayer = 100f;
        [SerializeField] private float intensity = 5f;
        [SerializeField] private float enableDistance = 1000000f;
        [SerializeField] private float minScale = 0.01f;

        private MeshFilter _meshFilter;
        private Transform _starTransform;
        private float _starRadius;
        
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            InitializePropBlock();
        }

        private void OnEnable()
        {
            playerMovement.OnCenter += SyncStarPosition;
        }
        
        private void OnDisable()
        {
            playerMovement.OnCenter -= SyncStarPosition;
        }

        private void Start()
        {
            var mesh = CubeSphereGenerator.Generate(resolution, radius);
            _meshFilter.sharedMesh = mesh;

            var star = systemGenerator.GetBodyByIndex(Vector2.zero);

            var data = star.GetBodyData();
            _starRadius = data.Radius;
            _starTransform = star.transform;

            MeshRenderer.GetPropertyBlock(MaterialPropertyBlock);
            MaterialPropertyBlock.SetVector(ShaderParametersIDs.StarColor, data.BaseColor);
            MaterialPropertyBlock.SetFloat(ShaderParametersIDs.StarIntensity, intensity);
            MeshRenderer.SetPropertyBlock(MaterialPropertyBlock);
        }

        private void LateUpdate()
        {
            SyncStarPosition();
        }
        
        private void SyncStarPosition()
        {
            if (!_starTransform || !playerTransform) return;

            float distanceToStar = Vector3.Distance(playerTransform.position, _starTransform.position);
            MeshRenderer.enabled = distanceToStar > enableDistance;

            if (!MeshRenderer.enabled || Mathf.Approximately(distanceToStar, 0f)) return;

            var directionToStar = (_starTransform.position - playerTransform.position).normalized;
            transform.position = playerTransform.position + directionToStar * distanceFromPlayer;

            float scale = _starRadius * (distanceFromPlayer / distanceToStar) / radius;

            scale = Mathf.Max(scale, minScale);
            
            transform.localScale = Vector3.one * scale;
            
        }
    }
}