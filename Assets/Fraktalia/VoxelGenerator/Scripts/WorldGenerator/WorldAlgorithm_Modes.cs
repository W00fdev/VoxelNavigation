using Fraktalia.VoxelGen;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using UnityEngine;

namespace Fraktalia.VoxelGen.World
{

	[BurstCompile]
	public static class WorldAlgorithm_Modes
	{
		[BurstCompile]
		public static void Set(ref NativeVoxelModificationData_Inner data, int value)
		{
			data.ID = value;
		}
		[BurstCompile]
		public static void Add(ref NativeVoxelModificationData_Inner data, int value)
		{
			data.ID += value;
		}
		[BurstCompile]
		public static void Subtract(ref NativeVoxelModificationData_Inner data, int value)
		{
			data.ID -= value;
		}
		[BurstCompile]
		public static void Min(ref NativeVoxelModificationData_Inner data, int value)
		{
			data.ID = Mathf.Min(data.ID, value);
		}
		[BurstCompile]
		public static void Max(ref NativeVoxelModificationData_Inner data, int value)
		{
			data.ID = Mathf.Max(data.ID, value);
		}

		[BurstCompile]
		public static void InvertSet(ref NativeVoxelModificationData_Inner data, int value)
		{
			data.ID = (255 - value);
		}
		[BurstCompile]
		public static void InvertAdd(ref NativeVoxelModificationData_Inner data, int value)
		{
			data.ID += (255 - value);
		}
		[BurstCompile]
		public static void InvertSubtract(ref NativeVoxelModificationData_Inner data, int value)
		{
			data.ID -= (255 - value);
		}
		[BurstCompile]
		public static void InvertMin(ref NativeVoxelModificationData_Inner data, int value)
		{
			data.ID = Mathf.Min(data.ID, (255 - value));
		}
		[BurstCompile]
		public static void InvertMax(ref NativeVoxelModificationData_Inner data, int value)
		{
			data.ID = Mathf.Max(data.ID, (255 - value));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int CalculateApplyMode(ApplyMode mode, int currentValue, int input)
		{
			switch (mode)
			{
				case ApplyMode.Set:
					return input;
				case ApplyMode.Add:
					return currentValue + input;
				case ApplyMode.Subtract:
					return currentValue - input;
				case World.ApplyMode.Min:
					return Mathf.Min(currentValue, input);
				case World.ApplyMode.Max:
					return Mathf.Max(currentValue, input);
				case World.ApplyMode.Modulate:
					return (currentValue * input + 255) >> 8;
				case World.ApplyMode.Average:
					return (currentValue + input) >> 1;


				case World.ApplyMode.InvertSet:
					return (255 - input);
				case World.ApplyMode.InvertAdd:
					return currentValue + (255 - input);
				case ApplyMode.InvertSubtract:
					return currentValue - (255 - input);
				case World.ApplyMode.InvertMin:
					return Mathf.Min(currentValue, (255 - input));
				case World.ApplyMode.InvertMax:
					return Mathf.Max(currentValue, (255 - input));
				case World.ApplyMode.InvertModulate:
					return (currentValue * (255 - input) + 255) >> 8;
				case World.ApplyMode.InvertAverage:
					return (currentValue + (255 - input)) >> 1;
			}

			return currentValue;
		}

	}

	public delegate void WorldAlgorithm_Mode(ref NativeVoxelModificationData_Inner data, int value);
}