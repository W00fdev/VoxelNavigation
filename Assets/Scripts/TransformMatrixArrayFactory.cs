using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class TransformMatrixArrayFactory
{
    public static NativeArray<Matrix4x4> CreateFromVoxels(
        int filledCount,
        bool[,,] voxels,
        Allocator allocator = Allocator.Persistent
    )
    {
        int width = voxels.GetLength(0);
        int height = voxels.GetLength(1);
        int depth = voxels.GetLength(2);

        var positions = new NativeArray<float3>(filledCount, allocator);
        int index = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    if (voxels[x, y, z])
                    {
                        //int index = x + width * (y + height * z);
                        positions[index++] = new float3(x, y, z);
                    }
                }
            }
        }

        var matrices = CreateFromPositions(positions, allocator);
        positions.Dispose();

        return matrices;
    }

    public static NativeArray<Matrix4x4> CreateFromPositions(
        NativeArray<float3> positions,
        Allocator allocator = Allocator.Persistent)
    {
        var transformMatrixArray = new NativeArray<Matrix4x4>(positions.Length, allocator);
        var job = new InitializeMatrixJob
        {
            Positions = positions,
            TransformMatrixArray = transformMatrixArray
        };

        JobHandle jobHandle = job.Schedule(positions.Length, 64);
        jobHandle.Complete();

        return transformMatrixArray;
    }

    [BurstCompile]
    private struct InitializeMatrixJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> Positions;
        [WriteOnly] public NativeArray<Matrix4x4> TransformMatrixArray;

        public void Execute(int index)
        {
            float3 position = Positions[index];

            TransformMatrixArray[index] =
                Matrix4x4.TRS(
                    new Vector3(position.x, position.y, position.z),
                    Quaternion.identity,
                    Vector3.one
                );
        }
    }
}