namespace ProceduralPlanets.BaseMesh
{
    using UnityEngine;

    public static class CubeSphereGenerator
    {
        private const int Int16MaxResolution = 100;
        private const int NumberOfFaces = 6;

        public static Mesh Generate(int resolution, float radius)
        {
            var mesh = new Mesh();

            if (resolution > Int16MaxResolution) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            Vector3[] faceDirections =
            {
                Vector3.up, Vector3.down,
                Vector3.left, Vector3.right,
                Vector3.forward, Vector3.back
            };

            var verticesPerFaceCount = resolution * resolution;
            var vertices = new Vector3[verticesPerFaceCount * NumberOfFaces];

            var triangles = new int[(resolution - 1) * (resolution - 1) * NumberOfFaces * NumberOfFaces];

            var vertexIndex = 0;
            var triangleIndex = 0;

            for (var i = 0; i < 6; i++)
            {
                GenerateFace(faceDirections[i], resolution, ref vertices, ref triangles, ref vertexIndex, ref triangleIndex,
                    radius);
            }

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private static void GenerateFace(Vector3 localUp, int resolution, ref Vector3[] vertices, ref int[] triangles, ref int vertexIndex,
            ref int triangleIndex, float radius)
        {
            var axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            var axisB = Vector3.Cross(localUp, axisA);

            var startIndex = vertexIndex;

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var percent = new Vector2(x, y) / (resolution - 1);
                    var pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;

                    var pointOnSphere = Spherify(pointOnUnitCube) * radius;

                    vertices[vertexIndex++] = pointOnSphere;

                    if (x == resolution - 1 || y == resolution - 1) continue;
                    
                    var i = startIndex + x + y * resolution;

                    triangles[triangleIndex++] = i;
                    triangles[triangleIndex++] = i + resolution + 1;
                    triangles[triangleIndex++] = i + resolution;

                    triangles[triangleIndex++] = i;
                    triangles[triangleIndex++] = i + 1;
                    triangles[triangleIndex++] = i + resolution + 1;
                }
            }
        }

        private static Vector3 Spherify(Vector3 position)
        {
            var xSquared = position.x * position.x;
            var ySquared = position.y * position.y;
            var zSquared = position.z * position.z;

            var x = position.x * Mathf.Sqrt(1f - (ySquared / 2f) - (zSquared / 2f) + (ySquared * zSquared / 3f));
            var y = position.y * Mathf.Sqrt(1f - (xSquared / 2f) - (zSquared / 2f) + (xSquared * zSquared / 3f));
            var z = position.z * Mathf.Sqrt(1f - (xSquared / 2f) - (ySquared / 2f) + (xSquared * ySquared / 3f));

            return new Vector3(x, y, z);
        }
    }
}