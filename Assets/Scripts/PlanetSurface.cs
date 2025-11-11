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
        [SerializeField] [Range(0, 1)] private float persistence;
        [SerializeField] private float heightMultiplier;

        [Header("NormalMap")] [SerializeField] private int height;
        [SerializeField] private int width;
        [SerializeField] private int normalMapOctaves;
        [SerializeField] private float strength;


        [Header("Debug")] [SerializeField] private bool drawVertexSpheres;
        [SerializeField] private float vertexSphereRadius;

        private MeshFilter _meshFilter;


        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            GeneratePlanet();
        }

        private void GeneratePlanet()
        {
            if (_meshFilter == null)
            {
                _meshFilter = GetComponent<MeshFilter>();
            }

            var sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");

            _meshFilter.sharedMesh = Instantiate(sphereMesh);

            var planetMesh = _meshFilter.sharedMesh;

            planetMesh.vertices = OffsetVertices(planetMesh.vertices);

            Texture2D normalMap = GenerateNormalMap();

            var renderer = GetComponent<MeshRenderer>();

            if (renderer.sharedMaterial == null)
                renderer.sharedMaterial = new Material(Shader.Find("Standard"));

            renderer.sharedMaterial.SetTexture("_BumpMap", normalMap);
            renderer.sharedMaterial.EnableKeyword("_NORMALMAP");

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(renderer.sharedMaterial);
            UnityEditor.EditorUtility.SetDirty(normalMap);
#endif

            planetMesh.RecalculateNormals();
            planetMesh.RecalculateBounds();
        }

        private Texture2D GenerateNormalMap()
        {
            var NoiseMap = new NoiseMap(seed, scale, normalMapOctaves, persistence, lacunarity);

            Vector3[] points = new Vector3[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    points[y * width + x] = new Vector3(x, y, 0);
                }
            }

            var heightMap = NoiseMap.GenerateNoiseMap(points);

            var heightMap2D = new float[width, height];

            for (int i = 0; i < heightMap.Length; i++)
            {
                int x = i % width;
                int y = i / width;
                heightMap2D[x, y] = heightMap[i];
            }

            Texture2D normalMap = NormalMapGenerator.GenerateNormalMap(heightMap2D, strength);
            return normalMap;
        }

        private Vector3[] OffsetVertices(Vector3[] meshVertices)
        {
            Vector3[] offsetVertices = new Vector3[meshVertices.Length];

            var noiseMap = new NoiseMap(seed, scale, octaves, persistence, lacunarity);

            float[] noiseValues = noiseMap.GenerateNoiseMap(meshVertices);

            for (int i = 0; i < meshVertices.Length; i++)
            {
                float height = planetRadius + noiseValues[i] * planetRadius * heightMultiplier;
                Vector3 vertex = meshVertices[i].normalized * height;
                offsetVertices[i] = vertex;
            }

            return offsetVertices;
        }

        private void OnValidate()
        {
            planetRadius = Mathf.Max(0, planetRadius);
            scale = Mathf.Max(0.0001f, scale);
            octaves = Mathf.Max(1, octaves);
            lacunarity = Mathf.Max(1, lacunarity);
            persistence = Mathf.Clamp01(persistence);
            vertexSphereRadius = Mathf.Max(0.01f, vertexSphereRadius);

            if (Application.isPlaying) return;
            GeneratePlanet();
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