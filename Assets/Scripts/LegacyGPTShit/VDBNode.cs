using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
// Базовый класс для узла VDB
    public abstract class VDBNode
    {
        public Vector3Int BoundsMin { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; } = true;
    }

// Внутренний узел дерева
    public class VDBInternalNode : VDBNode
    {
        public VDBNode[] Children { get; private set; }
    
        public VDBInternalNode(int level, Vector3Int boundsMin, int childCount = 8)
        {
            Level = level;
            BoundsMin = boundsMin;
            Children = new VDBNode[childCount];
        }
    }

// Листовой узел с вокселями
    public class VDBLeafNode : VDBNode
    {
        public float[] Voxels { get; private set; }
        public int Resolution { get; private set; }
    
        public VDBLeafNode(int level, Vector3Int boundsMin, int resolution = 8)
        {
            Level = level;
            BoundsMin = boundsMin;
            Resolution = resolution;
            Voxels = new float[resolution * resolution * resolution];
        }
    
        public float GetVoxel(Vector3Int localPos)
        {
            int index = localPos.x + localPos.y * Resolution + localPos.z * Resolution * Resolution;
            return Voxels[index];
        }
    
        public void SetVoxel(Vector3Int localPos, float value)
        {
            int index = localPos.x + localPos.y * Resolution + localPos.z * Resolution * Resolution;
            Voxels[index] = value;
        }
    }
}