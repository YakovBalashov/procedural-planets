namespace ProceduralPlanets.BaseMesh
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class CubeSphereGenerator
    {
        private const int Int16MaxResolution = 100;
        private const int NumberOfFaces = 6;
        private const int RoundFactor = 4;

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

            var vertexDict = new Dictionary<Vector3, int>();
            var verticesList = new List<Vector3>();
            var trianglesList = new List<int>();

            for (var i = 0; i < NumberOfFaces; i++)
            {
                GenerateFace(faceDirections[i], resolution, radius, vertexDict, verticesList, trianglesList);
            }

            mesh.vertices = verticesList.ToArray();
            mesh.triangles = trianglesList.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private static void GenerateFace(
            Vector3 localUp, 
            int resolution, 
            float radius, 
            Dictionary<Vector3, int> vertexDict, 
            List<Vector3> verticesList, 
            List<int> trianglesList)
        {
            var axisA = new Vector3(localUp.y, localUp.z, localUp.x);
            var axisB = Vector3.Cross(localUp, axisA);

            int[][] faceIndices = new int[resolution][];
            for (int index = 0; index < resolution; index++)
            {
                faceIndices[index] = new int[resolution];
            }

            for (var y = 0; y < resolution; y++)
            {
                for (var x = 0; x < resolution; x++)
                {
                    var percent = new Vector2(x, y) / (resolution - 1);
                    var pointOnUnitCube = localUp + (percent.x - 0.5f) * 2 * axisA + (percent.y - 0.5f) * 2 * axisB;
                    var pointOnSphere = Spherify(pointOnUnitCube) * radius;

                    Vector3 key = new Vector3(
                        (float)Math.Round(pointOnSphere.x, RoundFactor),
                        (float)Math.Round(pointOnSphere.y, RoundFactor),
                        (float)Math.Round(pointOnSphere.z, RoundFactor)
                    );

                    if (!vertexDict.TryGetValue(key, out int sharedIndex))
                    {
                        sharedIndex = verticesList.Count;
                        verticesList.Add(pointOnSphere);
                        vertexDict.Add(key, sharedIndex);
                    }

                    faceIndices[x][y] = sharedIndex;
                }
            }

            for (var y = 0; y < resolution - 1; y++)
            {
                for (var x = 0; x < resolution - 1; x++)
                {
                    int i = faceIndices[x][y];
                    int right = faceIndices[x + 1][y];
                    int top = faceIndices[x][y + 1];
                    int topRight = faceIndices[x + 1][y + 1];

                    trianglesList.Add(i);
                    trianglesList.Add(topRight);
                    trianglesList.Add(top);

                    trianglesList.Add(i);
                    trianglesList.Add(right);
                    trianglesList.Add(topRight);
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