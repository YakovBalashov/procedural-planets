using UnityEngine;

namespace ProceduralPlanets
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class PlanetSurface : MonoBehaviour
    {
        [Header("Mesh")] [SerializeField] float planetRadius;
        [SerializeField] private int seed;
        [SerializeField] private float scale;
        [SerializeField] private int octaves;
        [SerializeField] private float lacunarity;
        [SerializeField] private float persistence;

        [Header("Debug")] [SerializeField] private bool drawVertexSpheres;
        [SerializeField] private float vertexSphereRadius;
        
        
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

            var noiseMap = new NoiseMap(seed, scale, octaves, persistence, lacunarity);

            float[] noiseValues = noiseMap.GenerateNoiseMap(meshVertices);

            for (int i = 0; i < meshVertices.Length; i++)
            {
                float height = planetRadius + noiseValues[i];
                Vector3 vertex = meshVertices[i].normalized * height;
                offsetVertices[i] = vertex;
            }

            return offsetVertices;
        }

        private void OnValidate()
        {
            Debug.Log("OnValidate called");
            planetRadius = Mathf.Max(0, planetRadius);
            scale = Mathf.Max(0.0001f, scale);
            octaves = Mathf.Max(1, octaves);
            lacunarity = Mathf.Max(1, lacunarity);
            persistence = Mathf.Clamp01(persistence);
            vertexSphereRadius = Mathf.Max(0.01f, vertexSphereRadius);
        }

        private void OnDrawGizmos()
        {
            if (!drawVertexSpheres) return;

            if (!Application.isPlaying) return;

            var mesh = GetComponent<MeshFilter>().sharedMesh;
            if (mesh == null) return;

            var vertices = mesh.vertices;

            Gizmos.color = Color.red;
            foreach (var vertex in vertices)
            {
                Gizmos.DrawSphere(transform.position + vertex, vertexSphereRadius);
            }
        }
    }
}