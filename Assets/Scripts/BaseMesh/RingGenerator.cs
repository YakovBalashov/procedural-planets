using UnityEngine;

namespace ProceduralPlanets.BaseMesh
{
    public static class RingGenerator
    {
        public static Mesh Generate(int segmentCount, float innerRadius, float outerRadius)
        {
            var vertices = new Vector3[segmentCount * 2];
            var triangles = new int[segmentCount * 6];

            for (var i = 0; i < segmentCount; i++)
            {
                var angle = (float) i / segmentCount * Mathf.PI * 2;
                var cos = Mathf.Cos(angle);
                var sin = Mathf.Sin(angle);

                vertices[i * 2] = new Vector3(cos * innerRadius, 0, sin * innerRadius);
                vertices[i * 2 + 1] = new Vector3(cos * outerRadius, 0, sin * outerRadius);

                var nextIndex = (i + 1) % segmentCount;
                triangles[i * 6] = i * 2;
                triangles[i * 6 + 1] = nextIndex * 2;
                triangles[i * 6 + 2] = i * 2 + 1;

                triangles[i * 6 + 3] = i * 2 + 1;
                triangles[i * 6 + 4] = nextIndex * 2;
                triangles[i * 6 + 5] = nextIndex * 2 + 1;
            }
            
            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
