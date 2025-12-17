using Fraktalia.Utility.NativeNoise;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;

namespace Fraktalia.VoxelGen.World
{
	public class WorldAlgorithm_PerlinNoise : WorldAlgorithm
	{
		public enum NoiseMode
		{
			Noise1D,
			Noise2D,
			Noise3D
		}

		public enum NoiseAlgorithm
        {
			Perlin,
			PerlinFast,
			Simplex,
			SimplexFast,
			Voronoi,
			VoronoiFast,
			Worley,
			WorleyFast
        }

		[Header("Fractal Settings")]
		public int Octaves = 4;
		public float Lacunarity = 2;
		public float Gain = 0.5f;

		[Header("Perlin Noise Settings")]
		public float Frequency;
		public float Amplitude;
		public Vector3 Offset;
		
		public NoiseMode Mode;
		public NoiseAlgorithm Algorithm;
		public float Falloff = 10;

		public float StartValue = 0;

		public VoronoiNoise_Native.VORONOI_COMBINATION VoronoiCombination;
		public VoronoiNoise_Native.VORONOI_DISTANCE VoronoiDistance;
		public float WorleyJitter;

		WorldAlgorithm_PerlinNoise_Calculate2D calculate_2D;
		WorldAlgorithm_PerlinNoise_Calculate3D calculate_3D;

		public override void Initialize(VoxelGenerator template)
		{
			int width = template.GetBlockWidth(Depth);
			int blocks = width * width * width;
		
			calculate_2D.Width = width;
			calculate_2D.Blocks = blocks;
			calculate_2D.Depth = (byte)Depth;
			calculate_2D.Algorithm = Algorithm;			
			calculate_3D.Algorithm = Algorithm;					 
			calculate_3D.Width = width;
			calculate_3D.Blocks = blocks;
			calculate_3D.Depth = (byte)Depth;
		}

		public override JobHandle Apply(Vector3 hash, VoxelGenerator targetGenerator, ref JobHandle handle)
		{
			int width = targetGenerator.GetBlockWidth(Depth);
			int blocks = width * width * width;
			NativeArray<NativeVoxelModificationData_Inner> voxeldata = worldGenerator.modificationReservoir.GetDataArray(Depth);
			calculate_2D.VoxelSize = targetGenerator.GetVoxelSize(Depth);
			calculate_3D.VoxelSize = targetGenerator.GetVoxelSize(Depth);
			calculate_2D.RootSize = targetGenerator.RootSize;
			calculate_3D.RootSize = targetGenerator.RootSize;

			calculate_2D.voxeldata = voxeldata;
			calculate_3D.voxeldata = voxeldata;

			calculate_2D.ApplyFunction = ApplyFunction;
			calculate_2D.PostProcessFunction = PostProcessFunction;
			calculate_3D.ApplyFunction = ApplyFunction;
			calculate_3D.PostProcessFunction = PostProcessFunction;
			switch (Mode)
			{
				case NoiseMode.Noise1D:
					break;
				case NoiseMode.Noise2D:
					calculate_2D.PositionOffset = hash * targetGenerator.RootSize + Offset * scale;
					calculate_2D.Frequency = Frequency / scale;
					calculate_2D.Amplitude = Amplitude * scale;
					calculate_2D.FallOff = (Falloff * 40) / scale;
					calculate_2D.Octaves = Octaves;
					calculate_2D.Lacunarity = Lacunarity;
					calculate_2D.Gain = Gain;
					calculate_2D.PermutationTable = worldGenerator.Permutation;
					calculate_2D.VoronoiCombination = VoronoiCombination;
					calculate_2D.VoronoiDistance = VoronoiDistance;
					calculate_2D.WorleyJitter = WorleyJitter;

					//Stopwatch stopwatch = Stopwatch.StartNew();		
					//calculate_2D.Schedule(calculate_2D.Blocks, calculate_2D.Blocks / SystemInfo.processorCount).Complete();		
					//stopwatch.Stop();
					//UnityEngine.Debug.Log($"Noise calculation took {stopwatch.Elapsed.TotalMilliseconds} ms");

					return calculate_2D.Schedule(calculate_2D.Blocks, calculate_2D.Blocks / SystemInfo.processorCount, handle);
				case NoiseMode.Noise3D:
					calculate_3D.PositionOffset = hash * targetGenerator.RootSize + Offset * scale;
					calculate_3D.Frequency = Frequency / scale;
					calculate_3D.Amplitude = Amplitude;
					calculate_3D.StartValue = StartValue;				
					calculate_3D.Octaves = Octaves;
					calculate_3D.Lacunarity = Lacunarity;
					calculate_3D.Gain = Gain;
					calculate_3D.PermutationTable = worldGenerator.Permutation;
					calculate_3D.VoronoiCombination = VoronoiCombination;
					calculate_3D.VoronoiDistance = VoronoiDistance;
					calculate_3D.WorleyJitter = WorleyJitter;

					//Stopwatch stopwatch = Stopwatch.StartNew();		
					//calculate_3D.Schedule(calculate_3D.Blocks, calculate_3D.Blocks / SystemInfo.processorCount).Complete();		
					//stopwatch.Stop();
					//UnityEngine.Debug.Log($"Noise calculation took {stopwatch.Elapsed.TotalMilliseconds} ms");

					return calculate_3D.Schedule(calculate_3D.Blocks, calculate_3D.Blocks / SystemInfo.processorCount, handle);
				default:
					break;
			}


			return handle;
		}
	}



	[BurstCompile]
	public unsafe struct WorldAlgorithm_PerlinNoise_Calculate2D : IJobParallelFor
	{
		[ReadOnly]
		public PermutationTable_Native PermutationTable;

		public int Octaves;
		public float Lacunarity;
		public float Gain;

		public float Frequency;
		public float Amplitude;

		public byte Depth;
		public float VoxelSize;
		public float RootSize;
		public int Width;
		public int Blocks;
		public Vector3 PositionOffset;

		public float FallOff;

		public NativeArray<NativeVoxelModificationData_Inner> voxeldata;
		
        internal ApplyMode ApplyFunction;
        internal PostProcess PostProcessFunction;
        internal WorldAlgorithm_PerlinNoise.NoiseAlgorithm Algorithm;
        internal VoronoiNoise_Native.VORONOI_COMBINATION VoronoiCombination;
        internal VoronoiNoise_Native.VORONOI_DISTANCE VoronoiDistance;
		internal float WorleyJitter;

        public void Execute(int index)
		{
			int x = index % Width;
			int y = (index - x) / Width % Width;
			int z = ((index - x) / Width - y) / Width;

			float Voxelpos_X = x * VoxelSize;
			float Voxelpos_Y = y * VoxelSize;
			float Voxelpos_Z = z * VoxelSize;
			Voxelpos_X += VoxelSize / 2;
			Voxelpos_Y += VoxelSize / 2;
			Voxelpos_Z += VoxelSize / 2;


			float Worldpos_X = PositionOffset.x + Voxelpos_X;
			float Worldpos_Y = PositionOffset.y + Voxelpos_Y;
			float Worldpos_Z = PositionOffset.z + Voxelpos_Z;

			float biome = 1;// WorldGeneratorBiome.GetFactorizedBiomeID(Worldpos_X, Worldpos_Y, Worldpos_Z, ref BiomeTable, ref PermutationTable, BiomeTargetID);
			if (biome < 0) return;

			NativeVoxelModificationData_Inner info = voxeldata[index];
			info.Depth = Depth;
			info.X = VoxelGenerator.ConvertLocalToInner(Voxelpos_X, RootSize);
			info.Y = VoxelGenerator.ConvertLocalToInner(Voxelpos_Y, RootSize);
			info.Z = VoxelGenerator.ConvertLocalToInner(Voxelpos_Z, RootSize);

			float height = 0;

			float frequency = Frequency;
			float amplitude = Amplitude;


			switch (Algorithm)
			{
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.Perlin:
					for (int i = 0; i < Octaves; i++)
					{
						height += PerlinNoise_Native.Sample2D(Worldpos_X, Worldpos_Z, frequency, amplitude, ref PermutationTable);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.PerlinFast:
					void* perm = PermutationTable.unsavePermutationTable.ToPointer();
					for (int i = 0; i < Octaves; i++)
					{
						height += PerlinNoise_Native.Sample2DUnsafeFast(Worldpos_X, Worldpos_Z, frequency, amplitude, perm, PermutationTable.Wrap);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.Simplex:
					for (int i = 0; i < Octaves; i++)
					{
						height += SimplexNoise_Native.Sample2D(Worldpos_X, Worldpos_Z, frequency, amplitude, ref PermutationTable);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.SimplexFast:
					void* perm2 = PermutationTable.unsavePermutationTable.ToPointer();
					for (int i = 0; i < Octaves; i++)
					{
						height += SimplexNoise_Native.Sample2DUnsafeFast(Worldpos_X, Worldpos_Z, frequency, amplitude, perm2, PermutationTable.Wrap);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.Voronoi:
					for (int i = 0; i < Octaves; i++)
					{
						height += VoronoiNoise_Native.Sample2D(Worldpos_X, Worldpos_Z, frequency, amplitude, ref PermutationTable, VoronoiDistance, VoronoiCombination);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.VoronoiFast:
					void* perm3 = PermutationTable.unsavePermutationTable.ToPointer();
					for (int i = 0; i < Octaves; i++)
					{
						height += VoronoiNoise_Native.Sample2DUnsafeFast(Worldpos_X, Worldpos_Z, frequency, amplitude, perm3, PermutationTable.Wrap, PermutationTable.Inverse, VoronoiDistance, VoronoiCombination);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.Worley:
					for (int i = 0; i < Octaves; i++)
					{
						height += WorleyNoise_Native.Sample2D(Worldpos_X, Worldpos_Z, frequency, amplitude, WorleyJitter, ref PermutationTable, VoronoiDistance, VoronoiCombination);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}

					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.WorleyFast:
					void* perm4 = PermutationTable.unsavePermutationTable.ToPointer();
					for (int i = 0; i < Octaves; i++)
					{
						height += WorleyNoise_Native.Sample2DUnsafeFast(Worldpos_X, Worldpos_Z, frequency, amplitude, WorleyJitter, perm4, PermutationTable.Wrap, VoronoiDistance, VoronoiCombination);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
			}
         
			int value;
			if (Worldpos_Y < height)
			{
				value = 255;
			}
			else
			{
				float rest = Worldpos_Y - height;
				value = (int)(255 - rest * FallOff);
			}

			value = (int)(value * biome);

			value = WorldAlgorithm_PostProcesses.CalculateApplyMode(PostProcessFunction, value);
		    value = WorldAlgorithm_Modes.CalculateApplyMode(ApplyFunction, info.ID, value);
			info.ID = Mathf.Clamp(value, 0, 255);
			voxeldata[index] = info;
		}
	}

	[BurstCompile]
	public unsafe struct WorldAlgorithm_PerlinNoise_Calculate3D : IJobParallelFor
	{
		[ReadOnly]
		public PermutationTable_Native PermutationTable;

		public int Octaves;
		public float Lacunarity;
		public float Gain;

		public float Frequency;
		public float Amplitude;
		
		public float StartValue;

		public byte Depth;
		public float VoxelSize;
		public float RootSize;
		public int Width;
		public int Blocks;
		public Vector3 PositionOffset;

		public NativeArray<NativeVoxelModificationData_Inner> voxeldata;
		internal ApplyMode ApplyFunction;
		internal PostProcess PostProcessFunction;
		internal WorldAlgorithm_PerlinNoise.NoiseAlgorithm Algorithm;
		internal VoronoiNoise_Native.VORONOI_COMBINATION VoronoiCombination;
		internal VoronoiNoise_Native.VORONOI_DISTANCE VoronoiDistance;
		internal float WorleyJitter;

		public void Execute(int index)
		{

			int x = index % Width;
			int y = (index - x) / Width % Width;
			int z = ((index - x) / Width - y) / Width;

			float Voxelpos_X = x * VoxelSize;
			float Voxelpos_Y = y * VoxelSize;
			float Voxelpos_Z = z * VoxelSize;
			Voxelpos_X += VoxelSize / 2;
			Voxelpos_Y += VoxelSize / 2;
			Voxelpos_Z += VoxelSize / 2;


			float Worldpos_X = PositionOffset.x + Voxelpos_X;
			float Worldpos_Y = PositionOffset.y + Voxelpos_Y;
			float Worldpos_Z = PositionOffset.z + Voxelpos_Z;

			float biome = 1;// WorldGeneratorBiome.GetFactorizedBiomeID(Worldpos_X, Worldpos_Y, Worldpos_Z, ref BiomeTable, ref PermutationTable, BiomeTargetID);
			if (biome < 0) return;

			NativeVoxelModificationData_Inner info = voxeldata[index];
			info.Depth = Depth;
			info.X = VoxelGenerator.ConvertLocalToInner(Voxelpos_X, RootSize);
			info.Y = VoxelGenerator.ConvertLocalToInner(Voxelpos_Y, RootSize);
			info.Z = VoxelGenerator.ConvertLocalToInner(Voxelpos_Z, RootSize);

			float height = StartValue;
			float frequency = Frequency;
			float amplitude = Amplitude;

			switch (Algorithm)
			{
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.Perlin:
					
					for (int i = 0; i < Octaves; i++)
					{
						height += PerlinNoise_Native.Sample3D(Worldpos_X, Worldpos_Y, Worldpos_Z, frequency, amplitude, ref PermutationTable);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.PerlinFast:
					void* perm = PermutationTable.unsavePermutationTable.ToPointer();
					for (int i = 0; i < Octaves; i++)
					{
						height += PerlinNoise_Native.Sample3DUnsafeFast(Worldpos_X, Worldpos_Y, Worldpos_Z, frequency, amplitude, perm, PermutationTable.Wrap);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.Simplex:
					for (int i = 0; i < Octaves; i++)
					{
						height += SimplexNoise_Native.Sample3D(Worldpos_X, Worldpos_Y, Worldpos_Z, frequency, amplitude, ref PermutationTable);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.SimplexFast:
					void* perm2 = PermutationTable.unsavePermutationTable.ToPointer();
					for (int i = 0; i < Octaves; i++)
					{
						height += SimplexNoise_Native.Sample3DUnsafeFast(Worldpos_X, Worldpos_Y, Worldpos_Z, frequency, amplitude, perm2, PermutationTable.Wrap);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.Voronoi:
					for (int i = 0; i < Octaves; i++)
					{
						height += VoronoiNoise_Native.Sample3D(Worldpos_X, Worldpos_Y, Worldpos_Z, frequency, amplitude, ref PermutationTable, VoronoiDistance, VoronoiCombination);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}

					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.VoronoiFast:
					void* perm3 = PermutationTable.unsavePermutationTable.ToPointer();
					for (int i = 0; i < Octaves; i++)
					{
						height += VoronoiNoise_Native.Sample3DUnsafeFast(Worldpos_X, Worldpos_Y, Worldpos_Z, frequency, amplitude, perm3, PermutationTable.Wrap, PermutationTable.Inverse, VoronoiDistance, VoronoiCombination);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.Worley:
					for (int i = 0; i < Octaves; i++)
					{
						height += WorleyNoise_Native.Sample3D(Worldpos_X, Worldpos_Y, Worldpos_Z, frequency, amplitude, WorleyJitter, ref PermutationTable, VoronoiDistance, VoronoiCombination);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}

					break;
				case WorldAlgorithm_PerlinNoise.NoiseAlgorithm.WorleyFast:
					void* perm4 = PermutationTable.unsavePermutationTable.ToPointer();
					for (int i = 0; i < Octaves; i++)
					{
						height += WorleyNoise_Native.Sample3DUnsafeFast(Worldpos_X, Worldpos_Y, Worldpos_Z, frequency, amplitude, WorleyJitter, perm4, PermutationTable.Wrap, VoronoiDistance, VoronoiCombination);
						frequency *= Lacunarity;
						amplitude *= Gain;
					}
					break;
			}


			

			int value = (int)(height);

			value = WorldAlgorithm_PostProcesses.CalculateApplyMode(PostProcessFunction, value);
			value = WorldAlgorithm_Modes.CalculateApplyMode(ApplyFunction, info.ID, value);
			info.ID = Mathf.Clamp(value, 0, 255);
			voxeldata[index] = info;
		}
	}
}
