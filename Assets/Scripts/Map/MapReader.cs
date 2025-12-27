using System.IO;
using UnityEngine;

namespace Map
{
    public class MapReader : MonoBehaviour
    {
        [SerializeField] MapDrawer _mapDrawer;

        public bool[,,] Read(bool cull, out int totalFilledVoxelsCount)
        {
            bool[,,] voxels = ReadVoxelFile("Assets/Scenes/DC3.3dmap", out int width, out int height, out int depth);

            totalFilledVoxelsCount = 0;

            // cull inner blocks
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        voxels[x, y, z] = voxels[x, y, z]
                     && cull
                     && IsVisible(
                            voxels,
                            x,
                            y,
                            z,
                            width,
                            height,
                            depth
                        );

                        if (voxels[x, y, z])
                            totalFilledVoxelsCount++;
                    }
                }
            }

            return voxels;
        }

        private bool IsVisible(bool[,,] voxels, int x, int y, int z, int width, int height, int depth)
        {
            if (x + 1 >= width
            || y + 1 >= height
            || y - 1 <= 0
            || x - 1 <= 0
            || z + 1 >= depth
            || z - 1 <= 0)
                voxels[x, y, z] = true;

            return !(voxels[x + 1, y, z]
             && voxels[x - 1, y, z]
             && voxels[x, y + 1, z]
             && voxels[x, y - 1, z]
             && voxels[x, y, z + 1]
             && voxels[x, y, z - 1]);
        }


        static bool[,,] ReadVoxelFile(string filePath, out int width, out int height, out int depth)
        {
            width = 0;
            height = 0;
            depth = 0;

            string[] lines = File.ReadAllLines(filePath);

            string[] headerParts = lines[0]
               .Split(' ');

            if (headerParts.Length < 4
             || headerParts[0] != "voxel")
            {
                Debug.LogError("Неверный формат файла. Ожидается: 'voxel width height depth'");
                return null;
            }

            if (!int.TryParse(headerParts[1], out width)
             || !int.TryParse(headerParts[2], out height)
             || !int.TryParse(headerParts[3], out depth))
            {
                Debug.LogError("Не удалось распарсить размеры вокселей");
                return null;
            }

            Debug.Log($"Размеры: {width}x{height}x{depth}, Всего вокселей: {width * height * depth:N0}");

            bool[,,] voxels = new bool[width, height, depth];
            int filledCount = 0;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i]
                   .Trim();

                if (string.IsNullOrEmpty(line))
                    continue;

                string[] parts = line.Split(' ');

                if (parts.Length < 3)
                    continue;

                // Парсим координаты
                if (int.TryParse(parts[0], out int x)
                 && int.TryParse(parts[1], out int y)
                 && int.TryParse(parts[2], out int z))
                {
                    // Проверяем границы
                    if (x >= 0
                     && x < width
                     && y >= 0
                     && y < height
                     && z >= 0
                     && z < depth)
                    {
                        voxels[x, y, z] = true;
                        filledCount++;
                    }
                    else
                    {
                        Debug.LogWarning($"Координаты вне диапазона: [{x}, {y}, {z}]");
                    }
                }
            }

            Debug.Log($"Загружено заполненных вокселей: {filledCount}");
            return voxels;
        }
    }
}
