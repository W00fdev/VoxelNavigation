using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System;
using Unity.Burst;
using Fraktalia.Core.Collections;
using Fraktalia.Core.Mathematics;
using Fraktalia.Core.Math;
using Fraktalia.Utility;

namespace Fraktalia.VoxelGen.Visualisation
{
	[Obsolete("This hull generator is obsolete! Use ModularUniformVisualHull instead. Its faster and provides much more flexibility!")]
	public class MarchingCubes_CPU_v2 : UniformGridVisualHull_SingleStep_Async
	{			
		[Header("Appearance Settings:")]

		[Range(0, 255)]
		public int SurfacePoint;
	
		public float UVPower = 1;
		public float SmoothAngle = 60;

		[Header("Texture Channels")]
		[Range(-1, 4)]
		public int TextureDimensionUV3 = -1;
		
		[Range(-1, 4)]
		public int TextureDimensionUV4 = -1;
	
		[Range(-1, 4)]
		public int TextureDimensionUV5 = -1;
		
		[Range(-1, 4)]
		public int TextureDimensionUV6 = -1;
		[Space]
		public float TexturePowerUV3 = 1;
		public float TexturePowerUV4 = 1;
		public float TexturePowerUV5 = 1;
		public float TexturePowerUV6 = 1;

		[Tooltip("Merges UV5 and UV6 as y value into UV3 and UV4. Often shaders only allow texcoord UV1-UV4.")]
		public bool MergeUVs;

		[Space]
		[Tooltip("Bakes barycentric colors into the mesh. Is required for seamless tesselation.")]
		public bool AddTesselationColors = true;

		

		MarchingCubes_CPU_v2_Calculation[] calculators_v2;
		JobHandle[] jobhandles;
		private int[] currentWorkTable;

		protected override void Initialize()
		{
			NumCores = Mathf.Clamp(NumCores, 1, 8);
			width = Mathf.ClosestPowerOfTwo(width);
			Cell_Subdivision = Mathf.ClosestPowerOfTwo(Cell_Subdivision);
			
			base.Initialize();
		}

		protected override void initializeCalculators()
		{	
			calculators_v2 = new MarchingCubes_CPU_v2_Calculation[NumCores];
			jobhandles = new JobHandle[NumCores];
			currentWorkTable = new int[NumCores];
			for (int i = 0; i < calculators_v2.Length; i++)
			{
				calculators_v2[i].Init(width, i);

				calculators_v2[i].Shrink = Shrink;
				calculators_v2[i].MergeUV = MergeUVs ? 1 : 0;					
				calculators_v2[i].data = engine.Data[0];
				int BorderedWidth = width + 3;
				calculators_v2[i].UniformGrid = ContainerStaticLibrary.GetArray_byte(BorderedWidth * BorderedWidth * BorderedWidth, i);

				if (TextureDimensionUV3 != -1 && TextureDimensionUV3 < engine.Data.Length)
				{
					calculators_v2[i].texturedata_UV3 = engine.Data[TextureDimensionUV3];
				}

				if (TextureDimensionUV4 != -1 && TextureDimensionUV4 < engine.Data.Length)
				{
					calculators_v2[i].texturedata_UV4 = engine.Data[TextureDimensionUV4];
				}

				if (TextureDimensionUV5 != -1 && TextureDimensionUV5 < engine.Data.Length)
				{
					calculators_v2[i].texturedata_UV5 = engine.Data[TextureDimensionUV5];
				}

				if (TextureDimensionUV6 != -1 && TextureDimensionUV6 < engine.Data.Length)
				{
					calculators_v2[i].texturedata_UV6 = engine.Data[TextureDimensionUV6];
				}
			}
		}



		protected override IEnumerator beginCalculationasync(Queue<int> WorkerQueue, float cellSize, float voxelSize)
		{
			int works = 0;

			for (int i = 0; i < jobhandles.Length; i++)
			{	
				jobhandles[i].Complete();
			}

			for (int cores = 0; cores < calculators_v2.Length; cores++)
			{
				if (WorkerQueue.Count == 0) break;
				works++;

				int cellindex = WorkerQueue.Dequeue();
				int i = cellindex % Cell_Subdivision;
				int j = (cellindex - i) / Cell_Subdivision % Cell_Subdivision;
				int k = ((cellindex - i) / Cell_Subdivision - j) / Cell_Subdivision;
				float startX = i * cellSize;
				float startY = j * cellSize;
				float startZ = k * cellSize;

				Vector3Int offset = new Vector3Int();
				offset.x = NativeVoxelTree.CalculateInnerOffset(i, Cell_Subdivision);
				offset.y = NativeVoxelTree.CalculateInnerOffset(j, Cell_Subdivision);
				offset.z = NativeVoxelTree.CalculateInnerOffset(k, Cell_Subdivision);

				int innerVoxelSize = NativeVoxelTree.ConvertLocalToInner(voxelSize, engine.RootSize);
				calculators_v2[cores].voxelSizeBitPosition = MathUtilities.RightmostBitPosition(innerVoxelSize);
				calculators_v2[cores].voxelSize = cellSize / (width);
				calculators_v2[cores].cellSize = cellSize;
				calculators_v2[cores].positionoffset = new Vector3(startX, startY, startZ);
				calculators_v2[cores].positionoffset_Inner = new Fint3(offset.x, offset.y, offset.z);
				calculators_v2[cores].SmoothingAngle = SmoothAngle;
				calculators_v2[cores].Surface = SurfacePoint;
				calculators_v2[cores].UVPower = UVPower;
				calculators_v2[cores].TexturePowerUV3 = TexturePowerUV3;
				calculators_v2[cores].TexturePowerUV4 = TexturePowerUV4;
				calculators_v2[cores].TexturePowerUV5 = TexturePowerUV5;
				calculators_v2[cores].TexturePowerUV6 = TexturePowerUV6;
				jobhandles[cores] = calculators_v2[cores].Schedule();
				currentWorkTable[cores] = cellindex;
			}

			int synchronity = 0;
            for (int coreIndex = 0; coreIndex < works; coreIndex++)
            {
				while (!jobhandles[coreIndex].IsCompleted)
				{
					if (synchronity < frameBudgetPerCell)
					{
						synchronity++;
						yield return new YieldInstruction();
					}
					else
					{
						break;
					}
				}
				jobhandles[coreIndex].Complete();

				int cellindex = currentWorkTable[coreIndex];
				Haschset[cellindex] = false;
				VoxelPiece piece = VoxelMeshes[cellindex];
				if (piece == null)
				{
					continue;
				}

				piece.Clear();

				FNativeList<Vector3> vertices;
				FNativeList<int> triangles;
				FNativeList<Vector3> normals;

				finishCalculation(coreIndex, piece, out vertices, out triangles, out normals);
				
				if (!NoDistanceCollision || engine.CurrentLOD < FarDistance)
					piece.EnableCollision(!NoCollision);

				for (int x = 0; x < DetailGenerator.Count; x++)
				{
					var detail = DetailGenerator[x];
					if (detail)
					{
						if (detail.IsSave() && detail.enabled)
						{
							detail.DefineSurface(piece, vertices, triangles, normals, cellindex);
							detail.SetSlotDirty(cellindex);
							detail.PrepareWorks();
							detail.CompleteWorks();
						}
					}
				}
			}

			yield return null;
		}

		protected override void finishCalculation(int coreindex, VoxelPiece piece, out FNativeList<Vector3> vertices, out FNativeList<int> triangles, out FNativeList<Vector3> normals)
		{
			var usedcalculatorv2 = calculators_v2[coreindex];
			if (usedcalculatorv2.verticeArray.Length != 0)
			{
				piece.SetVertices(usedcalculatorv2.verticeArray);
				piece.SetTriangles(usedcalculatorv2.triangleArray);
				piece.SetNormals(usedcalculatorv2.normalArray);
				piece.SetTangents(usedcalculatorv2.tangentsArray);
				piece.SetUVs(0, usedcalculatorv2.uvArray);

				if (TextureDimensionUV3 != -1)
					piece.SetUVs(2, usedcalculatorv2.uv3Array);

				if (TextureDimensionUV4 != -1)
					piece.SetUVs(3, usedcalculatorv2.uv4Array);

				if (TextureDimensionUV5 != -1)
					piece.SetUVs(4, usedcalculatorv2.uv5Array);

				if (TextureDimensionUV6 != -1)
					piece.SetUVs(5, usedcalculatorv2.uv6Array);

				if (AddTesselationColors)
				{
					piece.SetColors(calculators_v2[coreindex].colorArray);
				}
			}

			vertices = usedcalculatorv2.verticeArray;
			triangles = usedcalculatorv2.triangleArray;
			normals = usedcalculatorv2.normalArray;
		}

		protected override void cleanUpCalculation()
		{	
			if (calculators_v2 != null)
			{
				for (int i = 0; i < calculators_v2.Length; i++)
				{
					jobhandles[i].Complete();	
				}
			}
		}

		protected override float GetChecksum()
		{
			float details = 0;

			BasicSurfaceModifier.RemoveDuplicates(DetailGenerator);

			for (int i = 0; i < DetailGenerator.Count; i++)
			{
				if(DetailGenerator[i])
				details += DetailGenerator[i].GetChecksum();
			}

			return base.GetChecksum() + Cell_Subdivision * 1000 + width * 100 + NumCores * 10 + UVPower + SmoothAngle + SurfacePoint
				+ TexturePowerUV3 + TexturePowerUV4 + TexturePowerUV5 + TexturePowerUV6+details;
		}
	}

}
