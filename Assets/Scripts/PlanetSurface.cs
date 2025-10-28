using UnityEngine;

namespace ProceduralPlanets
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetSurface : MonoBehaviour
    {
        [Header("Mesh")] [SerializeField] float planetRadius;
        [SerializeField] private float scale;
        [Header("Debug")] [SerializeField] private float vertexSphereRadius;


        private void Awake()
        {
            var meshFilter = GetComponent<MeshFilter>();
    
            var sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

            meshFilter.mesh = Instantiate(sphereMesh);

            var planetMesh = meshFilter.mesh;

            planetMesh.vertices = OffsetVertices(planetMesh.vertices);

            planetMesh.RecalculateNormals();
            planetMesh.RecalculateBounds();
            
        }

        private Vector3[] OffsetVertices(Vector3[] meshVertices)
        {
            Vector3[] offsetVertices = new Vector3[meshVertices.Length];
            
            var noiseMap = new NoiseMap(0,0,0,0);

            for (int i = 0; i < meshVertices.Length; i++)
            {
                float height = planetRadius + noiseMap.GenerateNoise(meshVertices[i]);
                Vector3 vertex = meshVertices[i].normalized * height;
                offsetVertices[i] = vertex;
            }

            return offsetVertices;
        }

        /*
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            var mesh = GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null)
                return;

            var vertices = mesh.vertices;

            Gizmos.color = Color.red;
            foreach (var vertex in vertices)
            {
                Gizmos.DrawSphere(transform.position + vertex, vertexSphereRadius);
            }
        }
    */
    }
}