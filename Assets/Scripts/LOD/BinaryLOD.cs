using UnityEngine;

namespace ProceduralPlanets.LOD
{
    public class BinaryLOD : MonoBehaviour
    {
        [SerializeField] private float switchDistance = 10f;
        
        private MeshRenderer _meshRenderer;
        private static Transform _cameraTransform;
        
        private void Awake()
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            if (!_cameraTransform) _cameraTransform = UnityEngine.Camera.main?.transform;
        }

        private void Update()
        {
            if (!_cameraTransform) return;
            var distanceToCamera = Vector3.Distance(transform.position, _cameraTransform.position);
            _meshRenderer.enabled = distanceToCamera <= switchDistance;
        }
    }
}
