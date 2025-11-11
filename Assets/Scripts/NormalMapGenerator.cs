using UnityEngine;

namespace ProceduralPlanets
{
    public class NormalMapGenerator
    {
        public static Texture2D GenerateNormalMap(float[,] heightMap, float strength)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            Texture2D normalMap = new Texture2D(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float heightL = heightMap[Mathf.Max(x - 1, 0), y];
                    float heightR = heightMap[Mathf.Min(x + 1, width - 1), y];
                    float heightD = heightMap[x, Mathf.Max(y - 1, 0)];
                    float heightU = heightMap[x, Mathf.Min(y + 1, height - 1)];

                    float dx = (heightR - heightL) * strength;
                    float dy = (heightU - heightD) * strength;

                    Vector3 normal = new Vector3(-dx, -dy, 1f).normalized;

                    Color color = new Color(
                        normal.x * 0.5f + 0.5f,
                        normal.y * 0.5f + 0.5f,
                        normal.z * 0.5f + 0.5f
                    );

                    normalMap.SetPixel(x, y, color);
                }
            }

            normalMap.Apply();
            return normalMap;
        }
    }
}