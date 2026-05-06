using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshGenerator : MonoBehaviour
    {
        [SerializeField] private Material material;
        
        protected MeshRenderer MeshRenderer;
        protected MaterialPropertyBlock MaterialPropertyBlock;

        protected void InitializePropBlock()
        {
            if (!MeshRenderer) MeshRenderer = GetComponent<MeshRenderer>();
            if (!MeshRenderer.sharedMaterial) MeshRenderer.sharedMaterial = material;
            if (MaterialPropertyBlock is null) MaterialPropertyBlock = new MaterialPropertyBlock();
        }
    }
}
