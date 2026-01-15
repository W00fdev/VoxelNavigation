using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    public static class ShapesFactory
    {
        static int SquaredDistance(Vector3Int a, Vector3Int b)
        {
            return SquaredDistance(a.x, a.y, a.z, b.x, b.y, b.z);
        }

        static int SquaredDistance(int ax, int ay, int az, Vector3Int b)
        {
            return SquaredDistance(ax, ay, az, b.x, b.y, b.z);
        }

        static int SquaredDistance(int ax, int ay, int az, int bx, int by, int bz)
        {
            return (ax - bx) * (ax - bx) + (ay - by) * (ay - by) + (az - bz) * (az - bz);
        }

        public static void CreateSphere(ref bool[,,] map, Vector3Int center, float radius)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);
            int depth = map.GetLength(2);

            // define bounding cube around sphere
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - radius));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(center.x + radius));

            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - radius));
            int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(center.y + radius));

            int minZ = Mathf.Max(0, Mathf.FloorToInt(center.z - radius));
            int maxZ = Mathf.Min(depth - 1, Mathf.CeilToInt(center.z + radius));

            // bound by map
            minX = Mathf.Clamp(minX, 0, width - 1);
            maxX = Mathf.Clamp(maxX, 0, width - 1);

            minY = Mathf.Clamp(minY, 0, height - 1);
            maxY = Mathf.Clamp(maxY, 0, height - 1);

            minZ = Mathf.Clamp(minZ, 0, depth - 1);
            maxZ = Mathf.Clamp(maxZ, 0, depth - 1);

            float radiusSquared = Mathf.Pow(radius, 2f);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        float distanceToCenterSquared = SquaredDistance(x, y, z, center);

                        map[x, y, z] = distanceToCenterSquared <= radiusSquared;
                    }
                }
            }
        }

        public static void CreateBox(ref bool[,,] map, Vector3Int center, Vector3Int bounds)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);
            int depth = map.GetLength(2);

            // define bounding cube around sphere
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - bounds.x));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(center.x + bounds.x));

            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - bounds.y));
            int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(center.y + bounds.y));

            int minZ = Mathf.Max(0, Mathf.FloorToInt(center.z - bounds.z));
            int maxZ = Mathf.Min(depth - 1, Mathf.CeilToInt(center.z + bounds.z));

            // bound by map
            minX = Mathf.Clamp(minX, 0, width - 1);
            maxX = Mathf.Clamp(maxX, 0, width - 1);

            minY = Mathf.Clamp(minY, 0, height - 1);
            maxY = Mathf.Clamp(maxY, 0, height - 1);

            minZ = Mathf.Clamp(minZ, 0, depth - 1);
            maxZ = Mathf.Clamp(maxZ, 0, depth - 1);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        map[x, y, z] = true;
                    }
                }
            }
        }

        public static void CreateTorus(ref bool[,,] map, Vector3Int center, int biggerRadius, int smallerRadius)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);
            int depth = map.GetLength(2);

            int outerBoundingRadius = biggerRadius + smallerRadius;

            // define bounding cube around sphere
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - outerBoundingRadius));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(center.x + outerBoundingRadius));

            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - outerBoundingRadius));
            int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(center.y + outerBoundingRadius));

            int minZ = Mathf.Max(0, Mathf.FloorToInt(center.z - outerBoundingRadius));
            int maxZ = Mathf.Min(depth - 1, Mathf.CeilToInt(center.z + outerBoundingRadius));

            // bound by map
            minX = Mathf.Clamp(minX, 0, width - 1);
            maxX = Mathf.Clamp(maxX, 0, width - 1);

            minY = Mathf.Clamp(minY, 0, height - 1);
            maxY = Mathf.Clamp(maxY, 0, height - 1);

            minZ = Mathf.Clamp(minZ, 0, depth - 1);
            maxZ = Mathf.Clamp(maxZ, 0, depth - 1);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        Vector3 p = new(x - center.x, y - center.y, z - center.z);
                
                        // Вычисляем расстояние в плоскости XZ
                        float distanceXZ = Mathf.Sqrt(p.x * p.x + p.z * p.z);
                
                        // Создаем 2D вектор как в шейдере
                        Vector2 q = new Vector2(distanceXZ - biggerRadius, p.y);
                
                        // Расстояние до тора
                        float distanceToTorus = q.magnitude - smallerRadius;
                
                        // Пороговое значение для вокселизации
                        float voxelThreshold = 0.5f; // Можно регулировать
                
                        // Если точка достаточно близко к поверхности тора
                        if (Mathf.Abs(distanceToTorus) <= voxelThreshold)
                        {
                            map[x, y, z] = true;
                        }
                    }
                }
            }
        }

        public static List<Vector3> CreateNoiseAt(ref bool[,,] map, Vector3Int center, Vector3Int bounds)
        {
            List<Vector3> addedVoxels = new();

            int width = map.GetLength(0);
            int height = map.GetLength(1);
            int depth = map.GetLength(2);

            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - bounds.x));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(center.x + bounds.x));

            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - bounds.y));
            int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(center.y + bounds.y));

            int minZ = Mathf.Max(0, Mathf.FloorToInt(center.z - bounds.z));
            int maxZ = Mathf.Min(depth - 1, Mathf.CeilToInt(center.z + bounds.z));

            // bound by map
            minX = Mathf.Clamp(minX, 0, width - 1);
            maxX = Mathf.Clamp(maxX, 0, width - 1);

            minY = Mathf.Clamp(minY, 0, height - 1);
            maxY = Mathf.Clamp(maxY, 0, height - 1);

            minZ = Mathf.Clamp(minZ, 0, depth - 1);
            maxZ = Mathf.Clamp(maxZ, 0, depth - 1);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        if (!map[x, y, z])
                        {
                            int u = x + z;
                            int v = y + z;

                            map[x, y, z] = Mathf.PerlinNoise(u, v) >= 0.5f;
                            addedVoxels.Add(new Vector3(x, y, z));
                        }
                    }
                }
            }

            return addedVoxels;
        }

        public static void CreateWireFrameBox(ref bool[,,] map, Vector3Int center, Vector3Int bounds)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);
            int depth = map.GetLength(2);

            // define bounding cube around sphere
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - bounds.x));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(center.x + bounds.x));

            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - bounds.y));
            int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(center.y + bounds.y));

            int minZ = Mathf.Max(0, Mathf.FloorToInt(center.z - bounds.z));
            int maxZ = Mathf.Min(depth - 1, Mathf.CeilToInt(center.z + bounds.z));

            // bound by map
            minX = Mathf.Clamp(minX, 0, width - 1);
            maxX = Mathf.Clamp(maxX, 0, width - 1);

            minY = Mathf.Clamp(minY, 0, height - 1);
            maxY = Mathf.Clamp(maxY, 0, height - 1);

            minZ = Mathf.Clamp(minZ, 0, depth - 1);
            maxZ = Mathf.Clamp(maxZ, 0, depth - 1);

            for (int x = minX; x <= maxX; x++)
            {
                map[x, minY, minZ] = true;
                map[x, minY, maxZ] = true;
                map[x, maxY, minZ] = true;
                map[x, maxY, maxZ] = true;
            }

            for (int y = minY; y <= maxY; y++)
            {
                map[minX, y, minZ] = true;
                map[maxX, y, minZ] = true;
                map[minX, y, maxZ] = true;
                map[maxX, y, maxZ] = true;
            }

            for (int z = minZ; z <= maxZ; z++)
            {
                map[minX, minY, z] = true;
                map[maxX, minY, z] = true;
                map[minX, maxY, z] = true;
                map[maxX, maxY, z] = true;
            }
        }
    }
}