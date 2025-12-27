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
            minX = Mathf.Clamp(minX, 0, width);
            maxX = Mathf.Clamp(maxX, 0, width);

            minY = Mathf.Clamp(minY, 0, height);
            maxY = Mathf.Clamp(maxY, 0, height);

            minZ = Mathf.Clamp(minZ, 0, depth);
            maxZ = Mathf.Clamp(maxZ, 0, depth);

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
            minX = Mathf.Clamp(minX, 0, width);
            maxX = Mathf.Clamp(maxX, 0, width);

            minY = Mathf.Clamp(minY, 0, height);
            maxY = Mathf.Clamp(maxY, 0, height);

            minZ = Mathf.Clamp(minZ, 0, depth);
            maxZ = Mathf.Clamp(maxZ, 0, depth);

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

        public static void CreateTorus(ref bool[,,] map, Vector3Int center, int outerRadius, int innerRadius)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);
            int depth = map.GetLength(2);

            int boundingRadius = outerRadius + innerRadius;

            // define bounding cube around sphere
            int minX = Mathf.Max(0, Mathf.FloorToInt(center.x - boundingRadius));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(center.x + boundingRadius));

            int minY = Mathf.Max(0, Mathf.FloorToInt(center.y - boundingRadius));
            int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(center.y + boundingRadius));

            int minZ = Mathf.Max(0, Mathf.FloorToInt(center.z - boundingRadius));
            int maxZ = Mathf.Min(depth - 1, Mathf.CeilToInt(center.z + boundingRadius));

            // bound by map
            minX = Mathf.Clamp(minX, 0, width);
            maxX = Mathf.Clamp(maxX, 0, width);

            minY = Mathf.Clamp(minY, 0, height);
            maxY = Mathf.Clamp(maxY, 0, height);

            minZ = Mathf.Clamp(minZ, 0, depth);
            maxZ = Mathf.Clamp(maxZ, 0, depth);


            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        float squaredDistance = SquaredDistance(x, y, z, center);

                        map[x, y, z] = squaredDistance <= outerRadius && squaredDistance >= innerRadius;
                    }
                }
            }
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
            minX = Mathf.Clamp(minX, 0, width);
            maxX = Mathf.Clamp(maxX, 0, width);

            minY = Mathf.Clamp(minY, 0, height);
            maxY = Mathf.Clamp(maxY, 0, height);

            minZ = Mathf.Clamp(minZ, 0, depth);
            maxZ = Mathf.Clamp(maxZ, 0, depth);

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