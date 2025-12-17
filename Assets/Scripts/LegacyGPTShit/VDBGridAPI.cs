using UnityEngine;

namespace DefaultNamespace
{
    public partial class SimpleVDBGrid
    {
        // Поиск листового узла для координат
        private VDBLeafNode FindLeafNode(Vector3Int voxelCoord)
        {
            return FindLeafNodeRecursive(_root, voxelCoord, _maxDepth);
        }

        private VDBLeafNode FindLeafNodeRecursive(VDBNode node, Vector3Int coord, int currentDepth)
        {
            if (node is VDBLeafNode leaf)
                return leaf;

            if (node is VDBInternalNode internalNode && currentDepth > 0)
            {
                int childIndex = CalculateChildIndex(coord, internalNode.BoundsMin, currentDepth);
                if (childIndex >= 0 && childIndex < internalNode.Children.Length &&
                    internalNode.Children[childIndex] != null)
                {
                    return FindLeafNodeRecursive(internalNode.Children[childIndex], coord, currentDepth - 1);
                }
            }

            return null;
        }

        // Поиск или создание листового узла
        private VDBLeafNode FindOrCreateLeafNode(Vector3Int voxelCoord)
        {
            return FindOrCreateLeafNodeRecursive(_root, voxelCoord, _maxDepth);
        }

        private VDBLeafNode FindOrCreateLeafNodeRecursive(VDBNode node, Vector3Int coord, int currentDepth)
        {
            if (currentDepth == 0) // Достигли уровня листьев
            {
                if (node is VDBLeafNode leaf)
                    return leaf;
                else
                    return CreateLeafNode(coord);
            }

            if (node is VDBInternalNode internalNode)
            {
                int childIndex = CalculateChildIndex(coord, internalNode.BoundsMin, currentDepth);

                // Вычисляем границы дочернего узла
                Vector3Int childBounds = CalculateChildBounds(internalNode.BoundsMin, childIndex, currentDepth);

                if (internalNode.Children[childIndex] == null)
                {
                    // Создаем новый узел (внутренний или листовой)
                    if (currentDepth > 1)
                    {
                        internalNode.Children[childIndex] = new VDBInternalNode(currentDepth - 1, childBounds);
                    }
                    else
                    {
                        internalNode.Children[childIndex] = CreateLeafNode(childBounds);
                    }
                }

                return FindOrCreateLeafNodeRecursive(internalNode.Children[childIndex], coord, currentDepth - 1);
            }

            return CreateLeafNode(coord);
        }

        private VDBLeafNode CreateLeafNode(Vector3Int boundsMin)
        {
            return new VDBLeafNode(0, boundsMin, _leafResolution);
        }

        // Вычисление индекса дочернего узла (октодерево)
        private int CalculateChildIndex(Vector3Int coord, Vector3Int nodeBounds, int level)
        {
            int levelSize = (int)Mathf.Pow(2, level) * _leafResolution;
            int halfSize = levelSize / 2;

            int xIndex = (coord.x - nodeBounds.x) >= halfSize ? 1 : 0;
            int yIndex = (coord.y - nodeBounds.y) >= halfSize ? 1 : 0;
            int zIndex = (coord.z - nodeBounds.z) >= halfSize ? 1 : 0;

            return xIndex + yIndex * 2 + zIndex * 4;
        }

        private Vector3Int CalculateChildBounds(Vector3Int parentBounds, int childIndex, int level)
        {
            int levelSize = (int)Mathf.Pow(2, level) * _leafResolution;
            int halfSize = levelSize / 2;

            int xOffset = (childIndex & 1) * halfSize;
            int yOffset = ((childIndex >> 1) & 1) * halfSize;
            int zOffset = ((childIndex >> 2) & 1) * halfSize;

            return parentBounds + new Vector3Int(xOffset, yOffset, zOffset);
        }

        private Vector3Int WorldToLocalVoxelCoord(Vector3Int worldCoord, Vector3Int leafBounds)
        {
            return new Vector3Int(
                worldCoord.x - leafBounds.x,
                worldCoord.y - leafBounds.y,
                worldCoord.z - leafBounds.z
            );
        }
    }
}