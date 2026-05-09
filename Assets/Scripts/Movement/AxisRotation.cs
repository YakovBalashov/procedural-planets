using ProceduralPlanets.Generation;
using UnityEngine;

namespace ProceduralPlanets.Movement
{
    public class AxisRotation : MonoBehaviour
    {
        [SerializeField] private Vector3 axis;
        [SerializeField] private float speedInDegreesPerSecond = 1f;

        private Quaternion _axisRotation;
        private float _currentAngle;
        private MaterialPropertyBlock _materialPropertyBlock;
        private MeshRenderer _meshRenderer;
        
        private void Awake()
        {
            AlignAxis();
        }

        private void OnValidate()
        {
            AlignAxis();
        }
        
        public void SetParameters(Vector3 newAxis, float newSpeedInDegreesPerSecond)
        {
            axis = newAxis;
            speedInDegreesPerSecond = newSpeedInDegreesPerSecond;
            AlignAxis();
        }
        
        private void AlignAxis()
        {
            _axisRotation = Quaternion.FromToRotation(Vector3.up, axis.normalized);
        }

        private void FixedUpdate()
        {
            _currentAngle += speedInDegreesPerSecond * Time.fixedDeltaTime * SystemGenerator.TimeMultiplier; 
            _currentAngle = Mathf.Repeat(_currentAngle, 360f);
            transform.rotation = _axisRotation * Quaternion.Euler(0f, _currentAngle, 0f);
            
            if (_meshRenderer is null) return;
            
            _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetFloat(ShaderParametersIDs.RotationAngle, _currentAngle);
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }

        public void SetMeshProperties(MaterialPropertyBlock materialPropertyBlock, MeshRenderer meshRenderer)
        {
            _materialPropertyBlock = materialPropertyBlock;
            _meshRenderer = meshRenderer;
            
            _meshRenderer.GetPropertyBlock(_materialPropertyBlock);
            _materialPropertyBlock.SetVector(ShaderParametersIDs.RotationAxis, axis.normalized);   
            _meshRenderer.SetPropertyBlock(_materialPropertyBlock);
        }
    }
}