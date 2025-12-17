using UnityEngine;

namespace DefaultNamespace
{
    public class VDBExample : MonoBehaviour
    {
        private SimpleVDBGrid _vdbGrid;

        void Start()
        {
            // Создаем VDB grid
            _vdbGrid = new SimpleVDBGrid("TestGrid", Vector3.zero, 1f, 4, 8);

            // Заполняем данными
            for (int x = -2; x < 2; x++)
            {
                for (int y = -2; y < 2; y++)
                {
                    for (int z = -2; z < 2; z++)
                    {
                        Vector3Int pos = new Vector3Int(x, y, z);
                        float value = Mathf.PerlinNoise(Mathf.Abs(x) / 2.0f, Mathf.Abs(y) / 2.0f);
                        _vdbGrid.SetVoxelByCoord(pos, value);
                    }
                }
            }

            // Навигация по вокселям
            /*Vector3 testPos = new Vector3(0.5f, 0.3f, 0.2f);
            float voxelValue = _vdbGrid.GetVoxel(testPos);
            Debug.Log($"Voxel at {testPos}: {voxelValue}");*/

            // Итерация по всем вокселям
            /*_vdbGrid.ForEachVoxel((pos, value) => {
                if (value > 0.5f)
                {
                    // Создаем визуализацию для плотных вокселей
                    CreateVoxelVisualization(pos, value);
                }
            });*/
        }

        void CreateVoxelVisualization(Vector3 position, float density)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            cube.transform.localScale = Vector3.one * 0.08f;

            // Цвет в зависимости от плотности
            Color color = Color.Lerp(Color.blue, Color.red, density);
            cube.GetComponent<Renderer>().material.color = color;
        }
    }
}