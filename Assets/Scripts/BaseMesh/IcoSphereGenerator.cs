using System.Collections.Generic;
using UnityEngine;

namespace ProceduralPlanets.BaseMesh
{
    public static class IcoSphereGenerator
    {
        public static Mesh Generate(int subdivisionNumber, float radius)
        {
            var vertices = new List<Vector3>(IcosahedronMeshData.Vertices);
            var triangles = new List<int>(IcosahedronMeshData.Triangles);
            
            var middlePointIndexCache = new Dictionary<long, int>();

            for (var i = 0; i < subdivisionNumber; i++)
            {
                Subdivide(vertices, triangles, middlePointIndexCache);
            }
            
            for (var i = 0; i < vertices.Count; i++)
            {
                vertices[i] = vertices[i].normalized * radius;
            }
            
            var mesh = new Mesh
            {
                vertices = vertices.ToArray(),
                triangles = triangles.ToArray()
            };
            
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void Subdivide(List<Vector3> vertices, List<int> triangles, Dictionary<long, int> cache)
        {
            var newTriangles = new List<int>();

            for (var i = 0; i < triangles.Count; i += 3)
            {
                var vertexA = triangles[i];
                var vertexB = triangles[i + 1];
                var vertexC = triangles[i + 2];

                var middlePointAB = GetMiddlePoint(vertexA, vertexB, vertices, cache);
                var middlePointBC = GetMiddlePoint(vertexB, vertexC, vertices, cache);
                var middlePointCA = GetMiddlePoint(vertexC, vertexA, vertices, cache);

                newTriangles.AddRange(new[]
                {
                    vertexA, middlePointAB, middlePointCA,
                    vertexB, middlePointBC, middlePointAB,
                    vertexC, middlePointCA, middlePointBC,
                    middlePointAB, middlePointBC, middlePointCA
                });
            }
            
            triangles.Clear();
            triangles.AddRange(newTriangles);
        }

        private static int GetMiddlePoint(int vertex1, int vertex2, List<Vector3> vertices, Dictionary<long, int> cache)
        {
            long smallerIndex = Mathf.Min(vertex1, vertex2);
            long greaterIndex = Mathf.Max(vertex1, vertex2);
            var key = (smallerIndex << 32) + greaterIndex;

            if (cache.TryGetValue(key, out var existing)) return existing;

            var middle = (vertices[vertex1] + vertices[vertex2]) / 2f;

            var index = vertices.Count;
            vertices.Add(middle);

            cache.Add(key, index);

            return index;
        }
    }
}