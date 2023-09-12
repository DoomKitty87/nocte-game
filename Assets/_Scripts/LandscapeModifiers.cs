using UnityEngine;

public static class LandscapeModifiers
{

    public static Vector3[] ApproximateHydroErosion(Vector3[] heightMap, int iterations, float intensity)
    {
        Vector3[] vertices = heightMap;

        float[] heightMapFloat = new float[vertices.Length];
        
        int size = (int)Mathf.Sqrt(vertices.Length);

        for (int i = 0; i < vertices.Length; i++)
        {
            heightMapFloat[i] = vertices[i].y;
        }
        
        for (int i = 0; i < iterations; i++)
        {
            float[] tempHeightMap = heightMapFloat;

            for (int j = 0; j < vertices.Length; j++)
            {
                float average = 0;
                bool skipped = false;

                for (int k = 0; k < 9; k++)
                {
                    if (k == 4) continue;
                    int neighbour = j - size - 1 + (k % 3) + (k / 3 * size);
                    if (neighbour >= vertices.Length || neighbour < 0 || neighbour % size == 0 || neighbour % size == size - 1)
                    {
                        skipped = true;
                        continue;
                    }
                    average += tempHeightMap[neighbour];
                }

                if (skipped) continue;
                average /= 8;
                
                heightMapFloat[j] += (average - tempHeightMap[j]) * intensity;
            }
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, heightMapFloat[i], vertices[i].z);
        }

        return vertices;
    }

    public static Vector3[] ContourMountains(Vector3[] heightMap)
    {
        Vector3[] vertices = heightMap;

        for (int i = 0; i < vertices.Length; i++)
        {
            
        }

        return vertices;
    }

}