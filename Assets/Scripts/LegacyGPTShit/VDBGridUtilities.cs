using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public partial class SimpleVDBGrid
    {
        // Получение всех активных листовых узлов
        public List<VDBLeafNode> GetAllLeafNodes()
        {
            var leaves = new List<VDBLeafNode>();
            CollectLeafNodesRecursive(_root, leaves);
            return leaves;
        }
    
        private void CollectLeafNodesRecursive(VDBNode node, List<VDBLeafNode> leaves)
        {
            if (node is VDBLeafNode leaf)
            {
                leaves.Add(leaf);
            }
            else if (node is VDBInternalNode internalNode)
            {
                foreach (var child in internalNode.Children)
                {
                    if (child != null)
                    {
                        CollectLeafNodesRecursive(child, leaves);
                    }
                }
            }
        }
    
        // Приблизительный аналог VDB-итератора
        public void ForEachVoxel(Action<Vector3, float> action)
        {
            foreach (var leaf in GetAllLeafNodes())
            {
                for (int x = 0; x < leaf.Resolution; x++)
                {
                    for (int y = 0; y < leaf.Resolution; y++)
                    {
                        for (int z = 0; z < leaf.Resolution; z++)
                        {
                            Vector3Int localCoord = new Vector3Int(x, y, z);
                            float value = leaf.GetVoxel(localCoord);
                        
                            if (value != 0f) // Только не-пустые воксели
                            {
                                Vector3Int worldVoxelCoord = leaf.BoundsMin + localCoord;
                                Vector3 worldPos = WorldOrigin + (Vector3)(worldVoxelCoord) * _voxelSize;
                                action(worldPos, value);
                            }
                        }
                    }
                }
            }
        }
    }
}