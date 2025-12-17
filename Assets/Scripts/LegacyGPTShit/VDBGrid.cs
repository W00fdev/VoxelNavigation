using UnityEngine;

namespace DefaultNamespace
{
    public partial class SimpleVDBGrid
    {
        private VDBInternalNode _root;
        private int _maxDepth;
        private float _voxelSize;
        private int _leafResolution;

        public string Name { get; set; }
        public Vector3 WorldOrigin { get; set; }

        public SimpleVDBGrid(string name, Vector3 worldOrigin, float voxelSize = 1.0f,
            int maxDepth = 4, int leafResolution = 8)
        {
            Name = name;
            WorldOrigin = worldOrigin;
            _voxelSize = voxelSize;
            _maxDepth = maxDepth;
            _leafResolution = leafResolution;

            // Создаем корневой узел
            int rootSize = (int)Mathf.Pow(2, maxDepth) * leafResolution;
            _root = new VDBInternalNode(maxDepth, Vector3Int.zero);
        }

        // Основной метод навигации и получения вокселя
        /*public float GetVoxel(Vector3 worldPosition)
        {
            Vector3 localPos = (worldPosition - WorldOrigin) / _voxelSize;
            Vector3Int voxelCoord = new Vector3Int(
                Mathf.FloorToInt(localPos.x),
                Mathf.FloorToInt(localPos.y),
                Mathf.FloorToInt(localPos.z)
            );

            return GetVoxelByCoord(voxelCoord);
        }*/

        public float GetVoxelByCoord(Vector3Int voxelCoord)
        {
            var leaf = FindLeafNode(voxelCoord);
            if (leaf != null)
            {
                Vector3Int localCoord = WorldToLocalVoxelCoord(voxelCoord, leaf.BoundsMin);
                return leaf.GetVoxel(localCoord);
            }

            return 0f; // Пустой воксель по умолчанию
        }

        /*public void SetVoxel(Vector3 worldPosition, float value)
        {
            Vector3 localPos = (worldPosition - WorldOrigin) / _voxelSize;
            Vector3Int voxelCoord = new Vector3Int(
                Mathf.FloorToInt(localPos.x),
                Mathf.FloorToInt(localPos.y),
                Mathf.FloorToInt(localPos.z)
            );

            SetVoxelByCoord(voxelCoord, value);
        }*/

        public void SetVoxelByCoord(Vector3Int voxelCoord, float value)
        {
            var leaf = FindOrCreateLeafNode(voxelCoord);
            Vector3Int localCoord = WorldToLocalVoxelCoord(voxelCoord, leaf.BoundsMin);
            leaf.SetVoxel(localCoord, value);
        }
    }
}