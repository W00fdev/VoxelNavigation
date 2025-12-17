using System.Collections;
using UnityEngine;
using Unity.Collections;
using System;
using Fraktalia.Core.FraktaliaAttributes;
using Unity.Jobs;
using Fraktalia.Core.Collections;
using Unity.Burst;

#if UNITY_EDITOR
#endif

namespace Fraktalia.VoxelGen.Visualisation
{
    [System.Serializable]
    public unsafe class SurfaceModifier_MultiTexture_V2 : SurfaceModifier
    {
        [PropertyKey("Layer Dimensions", false)]
        [Range(-1, 32)]
        public int Layer1Dimension = -1;

        [Range(-1, 32)]
        public int Layer2Dimension = -1;

        [Range(-1, 32)]
        public int Layer3Dimension = -1;

        [Range(-1, 32)]
        public int Layer4Dimension = -1;

       
        [Range(-1, 32)]
        public int Layer5Dimension = -1;

        [Range(-1, 32)]
        public int Layer6Dimension = -1;

        [Range(-1, 32)]
        public int Layer7Dimension = -1;

        [Range(-1, 32)]
        public int Layer8Dimension = -1;

        [Range(-1, 32)]
        public int Layer9Dimension = -1;

        [Range(-1, 32)]
        public int Layer10Dimension = -1;

        [Range(-1, 32)]
        public int Layer11Dimension = -1;

        [Range(-1, 32)]
        public int Layer12Dimension = -1;


        public JobHandle[] handles;
        public HullGenerationResult_MultiTexture_V2Job[] jobs;

        protected override void initializeModule()
        {
            handles = new JobHandle[HullGenerator.CurrentNumCores];
            jobs = new HullGenerationResult_MultiTexture_V2Job[HullGenerator.CurrentNumCores];
        }

        public override IEnumerator beginCalculationasync(float cellSize, float voxelSize)
        {
            for (int coreindex = 0; coreindex < HullGenerator.activeCores; coreindex++)
            {
                int cellIndex = HullGenerator.activeCells[coreindex];
                if (HullGenerator.WorkInformations[cellIndex].CurrentWorktype != ModularUniformVisualHull.WorkType.RequiresNonGeometryData) continue;

                if (HullGenerator.DebugMode)
                    Debug.Log("Modify Geometry");

                NativeMeshData data = HullGenerator.nativeMeshData[cellIndex];

                if (!handles[coreindex].IsCompleted)
                {
                    continue;
                }

                jobs[coreindex].verticeArray = data.verticeArray;
                jobs[coreindex].uv3Array = data.uv3Array;
                jobs[coreindex].uv4Array = data.uv4Array;
                jobs[coreindex].uv5Array = data.uv5Array;
                jobs[coreindex].uv6Array = data.uv6Array;

                // Existing layers
                if (Layer1Dimension != -1 && Layer1Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer1 = HullGenerator.engine.Data[Layer1Dimension];
                }

                if (Layer2Dimension != -1 && Layer2Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer2 = HullGenerator.engine.Data[Layer2Dimension];
                }

                if (Layer3Dimension != -1 && Layer3Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer3 = HullGenerator.engine.Data[Layer3Dimension];
                }

                if (Layer4Dimension != -1 && Layer4Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer4 = HullGenerator.engine.Data[Layer4Dimension];
                }

                // New layers (5-8)
                if (Layer5Dimension != -1 && Layer5Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer5 = HullGenerator.engine.Data[Layer5Dimension];
                }

                if (Layer6Dimension != -1 && Layer6Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer6 = HullGenerator.engine.Data[Layer6Dimension];
                }

                if (Layer7Dimension != -1 && Layer7Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer7 = HullGenerator.engine.Data[Layer7Dimension];
                }

                if (Layer8Dimension != -1 && Layer8Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer8 = HullGenerator.engine.Data[Layer8Dimension];
                }

                // New layers (9-12)
                if (Layer9Dimension != -1 && Layer9Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer9 = HullGenerator.engine.Data[Layer9Dimension];
                }

                if (Layer10Dimension != -1 && Layer10Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer10 = HullGenerator.engine.Data[Layer10Dimension];
                }

                if (Layer11Dimension != -1 && Layer11Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer11 = HullGenerator.engine.Data[Layer11Dimension];
                }

                if (Layer12Dimension != -1 && Layer12Dimension < HullGenerator.engine.Data.Length)
                {
                    jobs[coreindex].texturedata_layer12 = HullGenerator.engine.Data[Layer12Dimension];
                }

                handles[coreindex] = jobs[coreindex].Schedule();
            }

            for (int i = 0; i < HullGenerator.activeCores; i++)
            {
                handles[i].Complete();
            }

            yield return null;
        }

        [BurstCompile]
        public struct HullGenerationResult_MultiTexture_V2Job : IJob
        {
            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer1;

            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer2;

            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer3;

            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer4;

            // New texture data for additional layers (5-8)
            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer5;

            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer6;

            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer7;

            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer8;

            // New texture data for additional layers (9-12)
            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer9;

            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer10;

            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer11;

            [NativeDisableParallelForRestriction]
            public NativeVoxelTree texturedata_layer12;

            [ReadOnly]
            public FNativeList<Vector3> verticeArray;

            [NativeDisableParallelForRestriction]
            public FNativeList<Vector4> uv3Array;
            [NativeDisableParallelForRestriction]
            public FNativeList<Vector4> uv4Array;
            [NativeDisableParallelForRestriction]
            public FNativeList<Vector4> uv5Array;
            [NativeDisableParallelForRestriction]
            public FNativeList<Vector4> uv6Array;

            public void Execute()
            {
                int count = verticeArray.Length;
                Vector4 uvDataLayer3 = new Vector4(0, 0, 0, 0);
                Vector4 uvDataLayer4 = new Vector4(0, 0, 0, 0);
                Vector4 uvDataLayer5 = new Vector4(0, 0, 0, 0);
                uv3Array.Clear();
                uv4Array.Clear();
                uv5Array.Clear();

                for (int i = 0; i < count; i++)
                {
                    Vector3 vertex = verticeArray[i];

                    // Reset uvData for layers 1 to 4
                    uvDataLayer3 = new Vector4(0, 0, 0, 0);
                    if (texturedata_layer1.IsCreated)
                    {
                        uvDataLayer3.x = texturedata_layer1._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    if (texturedata_layer2.IsCreated)
                    {
                        uvDataLayer3.y = texturedata_layer2._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    if (texturedata_layer3.IsCreated)
                    {
                        uvDataLayer3.z = texturedata_layer3._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    if (texturedata_layer4.IsCreated)
                    {
                        uvDataLayer3.w = texturedata_layer4._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    // Store the first four layers in uv3Array
                    uv3Array.Add(uvDataLayer3 * 0.00390625f);

                    // Reset uvData for layers 5 to 8
                    uvDataLayer4 = new Vector4(0, 0, 0, 0);
                    if (texturedata_layer5.IsCreated)
                    {
                        uvDataLayer4.x = texturedata_layer5._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    if (texturedata_layer6.IsCreated)
                    {
                        uvDataLayer4.y = texturedata_layer6._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    if (texturedata_layer7.IsCreated)
                    {
                        uvDataLayer4.z = texturedata_layer7._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    if (texturedata_layer8.IsCreated)
                    {
                        uvDataLayer4.w = texturedata_layer8._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }
                
                    // Store layers 5 to 8 in uv4Array
                    uv4Array.Add(uvDataLayer4 * 0.00390625f);

                    // Add layers 9-12
                    uvDataLayer5 = new Vector4(0, 0, 0, 0);
                    if (texturedata_layer9.IsCreated)
                    {
                        uvDataLayer5.x = texturedata_layer9._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    if (texturedata_layer10.IsCreated)
                    {
                        uvDataLayer5.y = texturedata_layer10._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    if (texturedata_layer11.IsCreated)
                    {
                        uvDataLayer5.z = texturedata_layer11._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    if (texturedata_layer12.IsCreated)
                    {
                        uvDataLayer5.w = texturedata_layer12._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0);
                    }

                    uv5Array.Add(uvDataLayer5 * 0.00390625f);

                }
            }
        }

        internal override ModularUniformVisualHull.WorkType EvaluateWorkType(int dimension)
        {
            if (dimension == Layer1Dimension || dimension == Layer2Dimension || dimension == Layer3Dimension || dimension == Layer4Dimension || dimension == Layer5Dimension || dimension == Layer6Dimension || dimension == Layer7Dimension || dimension == Layer8Dimension)
                return ModularUniformVisualHull.WorkType.RequiresNonGeometryData;

            return ModularUniformVisualHull.WorkType.Nothing;
        }

        internal override void GetFractionalGeoChecksum(ref ModularUniformVisualHull.FractionalChecksum fractional, SurfaceModifierContainer container)
        {
            float sum = Layer1Dimension + Layer2Dimension + Layer3Dimension + Layer4Dimension + Layer5Dimension + Layer6Dimension + Layer7Dimension + Layer8Dimension;

            fractional.nongeometryChecksum += sum + (container.Disabled ? 0 : 1);
        }
    }
}
