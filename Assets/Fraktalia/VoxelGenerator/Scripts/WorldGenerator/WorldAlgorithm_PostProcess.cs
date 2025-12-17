using Fraktalia.VoxelGen;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using UnityEngine;

namespace Fraktalia.VoxelGen.World
{
	[BurstCompile]
	public static class WorldAlgorithm_PostProcesses
	{
		[BurstCompile]
		public static int Nothing(int value)
		{
			return value;
		}
		[BurstCompile]
		public static int NoNegatives(int value)
		{
			return Mathf.Max(0, value);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CalculateApplyMode(PostProcess mode, int currentValue)
		{
            switch (mode)
            {          
                case PostProcess.NoNegatives:
                    return Mathf.Max(0, currentValue);
			}

            return currentValue;
		}

	}

	public delegate int WorldAlgorithm_PostProcess(int value);
}