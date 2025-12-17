using UnityEngine;
using Fraktalia.Core.Collections;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Burst;
using Unity.Collections;
using System.Threading;
using System;

namespace Fraktalia.VoxelGen.Modify
{
    public unsafe class VM_PostProcessBehavior_Analyze : VM_PostProcessBehavior
    {
		public int DimensionToAnalyze;

		public int[] HistoGramm_Pre;
		public int[] HistoGramm_Post;
        public int[] HistoGramm_Analyzed;

		public override void FinalizeModification(FNativeList<NativeVoxelModificationData_Inner> modifierData,
			FNativeList<NativeVoxelModificationData_Inner> preVoxelData,
			FNativeList<NativeVoxelModificationData_Inner> postVoxelData, VoxelGenerator generator, VoxelModifier_V2 modifier)
		{
			int maxVoxelId = 256; // Set this according to your voxel ID range
			HistoGramm_Pre = new int[maxVoxelId];
			HistoGramm_Post = new int[maxVoxelId];
            HistoGramm_Analyzed = new int[maxVoxelId];

            if (DimensionToAnalyze < 0 || DimensionToAnalyze >= generator.Data.Length)
            {
                Debug.LogError("Invalid dimension to analyze. FinalizeModification will skip this post process.");
                return;
            }

			NativeArray<int> nativeHistogramPre = new NativeArray<int>(maxVoxelId, Allocator.TempJob);
			NativeArray<int> nativeHistogramPost = new NativeArray<int>(maxVoxelId, Allocator.TempJob);
            NativeArray<int> nativeHistogramAnalyzed = new NativeArray<int>(maxVoxelId, Allocator.TempJob);

            VM_PostProcessBehavior_AnalyzeJob job = new VM_PostProcessBehavior_AnalyzeJob
			{
				modifierData = modifierData,
				data = generator.Data[DimensionToAnalyze],
				preVoxelData = preVoxelData,
				postVoxelData = postVoxelData,
				histogramPre = nativeHistogramPre,
				histogramPost = nativeHistogramPost,
                histogramAnalyzed = nativeHistogramAnalyzed
			};

			job.Schedule(modifierData.Length, modifierData.Length / SystemInfo.processorCount).Complete();

            // Copy results back to managed arrays
            nativeHistogramPre.CopyTo(HistoGramm_Pre);
            nativeHistogramPost.CopyTo(HistoGramm_Post);
            nativeHistogramAnalyzed.CopyTo(HistoGramm_Analyzed);

            // Dispose native arrays
            nativeHistogramPre.Dispose();
            nativeHistogramPost.Dispose();
            nativeHistogramAnalyzed.Dispose();
        }

        [BurstCompile]
        public unsafe struct VM_PostProcessBehavior_AnalyzeJob : IJobParallelFor
        {
            public NativeVoxelTree data;

            [NativeDisableContainerSafetyRestriction]
            public FNativeList<NativeVoxelModificationData_Inner> modifierData;
            [NativeDisableContainerSafetyRestriction]
            public FNativeList<NativeVoxelModificationData_Inner> preVoxelData;
            [NativeDisableContainerSafetyRestriction]
            public FNativeList<NativeVoxelModificationData_Inner> postVoxelData;

            [NativeDisableParallelForRestriction]
            public NativeArray<int> histogramPre;
            [NativeDisableParallelForRestriction]
            public NativeArray<int> histogramPost;
            [NativeDisableParallelForRestriction]
            public NativeArray<int> histogramAnalyzed;

            public void Execute(int index)
            {
                NativeVoxelModificationData_Inner modifier = modifierData[index];
                NativeVoxelModificationData_Inner premodify = preVoxelData[index];
                NativeVoxelModificationData_Inner postmodify = postVoxelData[index];

                int preValue = premodify.ID;
                int postValue = postmodify.ID;
                int analyzedValue = data._PeekVoxelId_InnerCoordinate(modifier.X, modifier.Y, modifier.Z, 20, 0, -1);

                if (modifier.ID != -1)
                {
                    // Atomic increment
                    if (preValue >= 0 && preValue < histogramPre.Length)
                    {
                        AtomicIncrement(ref histogramPre, preValue);
                    }
                    if (postValue >= 0 && postValue < histogramPost.Length)
                    {
                        AtomicIncrement(ref histogramPost, postValue);
                    }
                    if (analyzedValue >= 0 && analyzedValue < histogramAnalyzed.Length)
                    {
                        AtomicIncrement(ref histogramAnalyzed, analyzedValue);
                    }
                }
            }

            private void AtomicIncrement(ref NativeArray<int> array, int index)
            {
                // Get the pointer to the array element
                IntPtr ptr = (IntPtr)(array.GetUnsafePtr());
                IntPtr location = IntPtr.Add(ptr, index * UnsafeUtility.SizeOf<int>());
                // Perform atomic increment
                Interlocked.Increment(ref *(int*)location);
            }
        }


    }

}
