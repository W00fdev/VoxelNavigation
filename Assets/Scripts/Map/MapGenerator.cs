using UnityEngine;

namespace Map
{
    public static class MapGenerator
    {
        public static bool[,,] CreateEmptyMap(Vector3Int size, bool filler = false)
        {
            return CreateEmptyMap(size.x, size.y, size.z, filler);
        }

        public static bool[,,] CreateEmptyMap(int width, int height, int depth, bool filler = false)
        {
            bool[,,] voxels = new bool[width, height, depth];

            if (!filler)
                return voxels;
            
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        voxels[x, y, z] = filler;
                    }
                }
            }

            return voxels;
        }

        public static bool[,,] GenerateTube()
        {
            // Размеры пространства согласно условию
            int sizeX = 75; // от 0 до 246 включительно
            int sizeY = 75; // от 0 до 154 включительно
            int sizeZ = 75; // от 0 до 205 включительно

            // Создаем трехмерный массив
            bool[,,] voxels = new bool[sizeX, sizeY, sizeZ];

            // Параметры трубы
            // Центр трубы
            float centerX = sizeX / 2f;
            float centerY = sizeY / 2f;

            // Внешний и внутренний радиусы трубы (воксели)
            float outerRadius = Mathf.Min(sizeX, sizeY) * 0.3f; // 30% от меньшего размера
            float innerRadius = outerRadius * 0.65f; // Внутренний радиус - половина внешнего

            // Высота трубы (по оси Z)
            float tubeHeight = sizeZ;

            // Заполняем массив вокселями трубы
            for (int x = 0; x < sizeX; x++)
            {
                for (int y = 0; y < sizeY; y++)
                {
                    for (int z = 0; z < sizeZ; z++)
                    {
                        // Вычисляем расстояние от центра
                        float distance = (float)Mathf.Sqrt(
                            Mathf.Pow(x - centerX, 2) +
                            Mathf.Pow(y - centerY, 2)
                        );

                        // Проверяем, находится ли точка в пределах трубы
                        // (между внутренним и внешним радиусом)
                        bool isInTube = distance >= innerRadius && distance <= outerRadius;

                        // Дополнительно: можно сделать трубу не на всю высоту
                        // Например, от 20% до 80% высоты
                        float startHeight = tubeHeight * 0.4f;
                        float endHeight = tubeHeight * 0.6f;
                        bool isInHeightRange = z >= startHeight && z <= endHeight;

                        // Устанавливаем воксель как часть трубы
                        // Если хотите трубу по всей высоте - используйте только isInTube
                        // Если хотите ограниченную по высоте - используйте isInTube && isInHeightRange
                        voxels[x, y, z] = isInTube && isInHeightRange;
                    }
                }
            }

            return voxels;
        }
    }
}
