using UnityEngine;

namespace ProceduralPlanets
{
    public class MeshGenerator : MonoBehaviour
    {
        [SerializeField] private Vector2 vertexCount;
        [SerializeField] private bool useRandomHeight;
        [SerializeField] private float heightMultiplier;
        [SerializeField] private float noiseScale;

        private int _vertexCountX;
        private int _vertexCountZ;

        private int _gridSizeX;
        private int _gridSizeZ;

        private void Awake()
        {
            _vertexCountX = (int)vertexCount.x;
            _vertexCountZ = (int)vertexCount.y;

            _gridSizeX = _vertexCountX - 1;
            _gridSizeZ = _vertexCountZ - 1;

            var mesh = new Mesh();

            var meshFilter = GetComponent<MeshFilter>();

            meshFilter.mesh = mesh;

            var vertices = GenerateVertices();
            var triangles = GenerateTriangles();

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
        }

        private int[] GenerateTriangles()
        {
            var triangles = new int[_gridSizeX * _gridSizeZ * 6];

            var triangleIndex = 0;

            for (var z = 0; z < _gridSizeZ; z++)
            {
                for (var x = 0; x < _gridSizeX; x++)
                {
                    var bottomLeft = z * _vertexCountX + x;
                    var bottomRight = bottomLeft + 1;
                    var topLeft = bottomLeft + _vertexCountX;
                    var topRight = topLeft + 1;

                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomRight;

                    triangles[triangleIndex++] = bottomRight;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topRight;
                }
            }

            return triangles;
        }

        private Vector3[] GenerateVertices()
        {
            var vertices = new Vector3[_vertexCountX * _vertexCountZ];

            for (int i = 0, z = 0; z < _vertexCountZ; z++)
            {
                for (var x = 0; x < _vertexCountX; x++)
                {
                    var height = useRandomHeight
                        ? Random.Range(-2, 2)
                        : Mathf.PerlinNoise(x * noiseScale, z * noiseScale) * heightMultiplier;
                    vertices[i] = new Vector3(x, height, z);
                    i++;
                }
            }

            return vertices;
        }
    }
}