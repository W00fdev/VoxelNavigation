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
	public unsafe class SurfaceModifier_MultiTexture : SurfaceModifier
	{
		[PropertyKey("Texture Settings X Coord", false)]
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

		[Header("Texture Settings Y Coord")]
		[Range(-1, 4)]
		public int TextureDimensionUV3Y = -1;

		[Range(-1, 4)]
		public int TextureDimensionUV4Y = -1;

		[Range(-1, 4)]
		public int TextureDimensionUV5Y = -1;

		[Range(-1, 4)]
		public int TextureDimensionUV6Y = -1;
		[Space]
		public float TexturePowerUV3Y = 1;
		public float TexturePowerUV4Y = 1;
		public float TexturePowerUV5Y = 1;
		public float TexturePowerUV6Y = 1;

		public JobHandle[] handles;
		public HullGenerationResult_MultiTextureJob[] jobs;
		protected override void initializeModule()
		{
			handles = new JobHandle[HullGenerator.CurrentNumCores];
			jobs = new HullGenerationResult_MultiTextureJob[HullGenerator.CurrentNumCores];
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

				if (TextureDimensionUV3 != -1 && TextureDimensionUV3 < HullGenerator.engine.Data.Length)
				{
					jobs[coreindex].texturedata_UV3 = HullGenerator.engine.Data[TextureDimensionUV3];
				}

				if (TextureDimensionUV4 != -1 && TextureDimensionUV4 < HullGenerator.engine.Data.Length)
				{
					jobs[coreindex].texturedata_UV4 = HullGenerator.engine.Data[TextureDimensionUV4];
				}

				if (TextureDimensionUV5 != -1 && TextureDimensionUV5 < HullGenerator.engine.Data.Length)
				{
					jobs[coreindex].texturedata_UV5 = HullGenerator.engine.Data[TextureDimensionUV5];
				}

				if (TextureDimensionUV6 != -1 && TextureDimensionUV6 < HullGenerator.engine.Data.Length)
				{
					jobs[coreindex].texturedata_UV6 = HullGenerator.engine.Data[TextureDimensionUV6];
				}

				jobs[coreindex].TexturePowerUV3 = TexturePowerUV3;
				jobs[coreindex].TexturePowerUV4 = TexturePowerUV4;
				jobs[coreindex].TexturePowerUV5 = TexturePowerUV5;
				jobs[coreindex].TexturePowerUV6 = TexturePowerUV6;


				if (TextureDimensionUV3Y != -1 && TextureDimensionUV3Y < HullGenerator.engine.Data.Length)
				{
					jobs[coreindex].texturedata_UV3Y = HullGenerator.engine.Data[TextureDimensionUV3Y];
				}

				if (TextureDimensionUV4Y != -1 && TextureDimensionUV4Y < HullGenerator.engine.Data.Length)
				{
					jobs[coreindex].texturedata_UV4Y = HullGenerator.engine.Data[TextureDimensionUV4Y];
				}

				if (TextureDimensionUV5Y != -1 && TextureDimensionUV5Y < HullGenerator.engine.Data.Length)
				{
					jobs[coreindex].texturedata_UV5Y = HullGenerator.engine.Data[TextureDimensionUV5Y];
				}

				if (TextureDimensionUV6Y != -1 && TextureDimensionUV6Y < HullGenerator.engine.Data.Length)
				{
					jobs[coreindex].texturedata_UV6Y = HullGenerator.engine.Data[TextureDimensionUV6Y];
				}

				jobs[coreindex].TexturePowerUV3Y = TexturePowerUV3Y;
				jobs[coreindex].TexturePowerUV4Y = TexturePowerUV4Y;
				jobs[coreindex].TexturePowerUV5Y = TexturePowerUV5Y;
				jobs[coreindex].TexturePowerUV6Y = TexturePowerUV6Y;
				
				handles[coreindex] = jobs[coreindex].Schedule();
			}

			for (int i = 0; i < HullGenerator.activeCores; i++)
			{
				//while (!handles[i].IsCompleted)
				//{
				//	if (HullGenerator.synchronitylevel < 0) break;
				//	yield return new YieldInstruction();
				//}
				handles[i].Complete();
			}

			yield return null;
		}

		[BurstCompile]
		public struct HullGenerationResult_MultiTextureJob : IJob
		{
			[NativeDisableParallelForRestriction]
			public NativeVoxelTree texturedata_UV3;

			[NativeDisableParallelForRestriction]
			public NativeVoxelTree texturedata_UV4;

			[NativeDisableParallelForRestriction]
			public NativeVoxelTree texturedata_UV5;

			[NativeDisableParallelForRestriction]
			public NativeVoxelTree texturedata_UV6;

			public float TexturePowerUV3;
			public float TexturePowerUV4;
			public float TexturePowerUV5;
			public float TexturePowerUV6;

			[NativeDisableParallelForRestriction]
			public NativeVoxelTree texturedata_UV3Y;

			[NativeDisableParallelForRestriction]
			public NativeVoxelTree texturedata_UV4Y;

			[NativeDisableParallelForRestriction]
			public NativeVoxelTree texturedata_UV5Y;

			[NativeDisableParallelForRestriction]
			public NativeVoxelTree texturedata_UV6Y;

			public float TexturePowerUV3Y;
			public float TexturePowerUV4Y;
			public float TexturePowerUV5Y;
			public float TexturePowerUV6Y;

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
				Vector2 uvData = new Vector2(0, 0);

				if (uv3Array.Length < count)
				{
					if (texturedata_UV3.IsCreated || texturedata_UV3Y.IsCreated)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vertex = verticeArray[i];
							uvData.x = 0;
							uvData.y = 0;

							if (texturedata_UV3.IsCreated)
							{
								Vector3Int inner = NativeVoxelTree.ConvertLocalToInner(vertex, texturedata_UV3.RootSize);
								uvData.x = texturedata_UV3._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0,0) * TexturePowerUV3;
							}

							if (texturedata_UV3Y.IsCreated)
							{
								Vector3Int inner = NativeVoxelTree.ConvertLocalToInner(vertex, texturedata_UV3Y.RootSize);
								uvData.y = texturedata_UV3Y._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0,0) * TexturePowerUV3;
							}


							uv3Array.Add(uvData);
						}
					}

					if (texturedata_UV4.IsCreated || texturedata_UV4Y.IsCreated)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vertex = verticeArray[i];
							uvData.x = 0;
							uvData.y = 0;

							if (texturedata_UV4.IsCreated)
							{
								Vector3Int inner = NativeVoxelTree.ConvertLocalToInner(vertex, texturedata_UV4.RootSize);
								uvData.x = texturedata_UV4._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0) * TexturePowerUV4;
							}

							if (texturedata_UV4Y.IsCreated)
							{
								Vector3Int inner = NativeVoxelTree.ConvertLocalToInner(vertex, texturedata_UV4Y.RootSize);
								uvData.y = texturedata_UV4Y._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0) * TexturePowerUV4Y;
							}

							uv4Array.Add(uvData);
						}
					}


					if (texturedata_UV5.IsCreated || texturedata_UV5Y.IsCreated)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vertex = verticeArray[i];
							uvData.x = 0;
							uvData.y = 0;

							if (texturedata_UV5.IsCreated)
							{
								Vector3Int inner = NativeVoxelTree.ConvertLocalToInner(vertex, texturedata_UV5.RootSize);
								uvData.x = texturedata_UV5._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0) * TexturePowerUV5;
							}

							if (texturedata_UV5Y.IsCreated)
							{
								uvData.y = texturedata_UV5Y._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV5Y;
							}

							uv5Array.Add(uvData);
						}
					}

					if (texturedata_UV6.IsCreated || texturedata_UV6Y.IsCreated)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vertex = verticeArray[i];
							uvData.x = 0;
							uvData.y = 0;

							if (texturedata_UV6.IsCreated)
							{
								uvData.x = texturedata_UV6._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV6;
							}

							if (texturedata_UV6Y.IsCreated)
							{
								uvData.y = texturedata_UV6Y._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV6Y;
							}

							uv6Array.Add(uvData);
						}
					}

					return;
				}

				if (uv3Array.Length == count)
				{
					if (texturedata_UV3.IsCreated || texturedata_UV3Y.IsCreated)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vertex = verticeArray[i];
							uvData.x = 0;
							uvData.y = 0;

							if (texturedata_UV3.IsCreated)
							{
								uvData.x = texturedata_UV3._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV3;
							}

							if (texturedata_UV3Y.IsCreated)
							{
								uvData.y = texturedata_UV3Y._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV3Y;
							}


							uv3Array[i] = (uvData);
						}
					}

					if (texturedata_UV4.IsCreated || texturedata_UV4Y.IsCreated)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vertex = verticeArray[i];
							uvData.x = 0;
							uvData.y = 0;

							if (texturedata_UV4.IsCreated)
							{
								uvData.x = texturedata_UV4._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV4;
							}

							if (texturedata_UV4Y.IsCreated)
							{
								uvData.y = texturedata_UV4Y._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV4Y;
							}

							uv4Array[i] = (uvData);
						}
					}


					if (texturedata_UV5.IsCreated || texturedata_UV5Y.IsCreated)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vertex = verticeArray[i];
							uvData.x = 0;
							uvData.y = 0;

							if (texturedata_UV5.IsCreated)
							{
								uvData.x = texturedata_UV5._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV5;
							}

							if (texturedata_UV5Y.IsCreated)
							{
								uvData.y = texturedata_UV5Y._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV5Y;
							}

							uv5Array[i] = (uvData);
						}
					}

					if (texturedata_UV6.IsCreated || texturedata_UV6Y.IsCreated)
					{
						for (int i = 0; i < count; i++)
						{
							Vector3 vertex = verticeArray[i];
							uvData.x = 0;
							uvData.y = 0;

							if (texturedata_UV6.IsCreated)
							{
								uvData.x = texturedata_UV6._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV6;
							}

							if (texturedata_UV6Y.IsCreated)
							{
								uvData.y = texturedata_UV6Y._PeekVoxelId(vertex.x, vertex.y, vertex.z, 10, 0) * TexturePowerUV6Y;
							}

							uv6Array[i] = (uvData);
						}
					}

					return;
				}



			}
		}

		internal override ModularUniformVisualHull.WorkType EvaluateWorkType(int dimension)
		{
			if (dimension == TextureDimensionUV3 || dimension == TextureDimensionUV4 || dimension == TextureDimensionUV5 || dimension == TextureDimensionUV6)
				return ModularUniformVisualHull.WorkType.RequiresNonGeometryData;

			if (dimension == TextureDimensionUV3Y || dimension == TextureDimensionUV4Y || dimension == TextureDimensionUV5Y || dimension == TextureDimensionUV6Y)
				return ModularUniformVisualHull.WorkType.RequiresNonGeometryData;


			return ModularUniformVisualHull.WorkType.Nothing;
		}

		internal override void GetFractionalGeoChecksum(ref ModularUniformVisualHull.FractionalChecksum fractional, SurfaceModifierContainer container)
		{
			float sum = TexturePowerUV3 + TexturePowerUV4 + TexturePowerUV5 + TexturePowerUV6 + TextureDimensionUV3 + TextureDimensionUV4 + TextureDimensionUV5 + TextureDimensionUV6;
			sum += TexturePowerUV3Y + TexturePowerUV4Y + TexturePowerUV5Y + TexturePowerUV6Y + TextureDimensionUV3Y + TextureDimensionUV4Y + TextureDimensionUV5Y + TextureDimensionUV6Y;
			fractional.nongeometryChecksum += sum + (container.Disabled ? 0 : 1);
		}
	}

	[System.Serializable]
	public unsafe class SurfaceModifier_AtlasUV : SurfaceModifier
	{
		[PropertyKey("Texture Settings X Coord", false)]

		
		[Range(1, 16)]
		public int AtlasRows = 2;
		[Range(1, 16)]
		[Tooltip("Groups Atlas lookup to set of vertices. 0, 3 and 6 looks good. Everythign else looks bad -,-")]
		public int AtlasModulo = 3;

		[Header("Texture Channels")]
		[Range(-1, 4)]
		public int AtlasTextureDimension = -1;

		public JobHandle[] handles;
		public HullGenerationResult_AtlasUVJob[] jobs;
		protected override void initializeModule()
		{
			handles = new JobHandle[HullGenerator.CurrentNumCores];
			jobs = new HullGenerationResult_AtlasUVJob[HullGenerator.CurrentNumCores];
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
				jobs[coreindex].uvArray = data.uvArray;
				jobs[coreindex].AtlasModulo = AtlasModulo;
				jobs[coreindex].AtlasRow = AtlasRows;

				if (AtlasTextureDimension != -1 && AtlasTextureDimension < HullGenerator.engine.Data.Length)
				{
					jobs[coreindex].atlasDimension = HullGenerator.engine.Data[AtlasTextureDimension];
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
		public struct HullGenerationResult_AtlasUVJob : IJob
		{
			

			[NativeDisableParallelForRestriction]
			public NativeVoxelTree atlasDimension;

		

			[ReadOnly]
			public FNativeList<Vector3> verticeArray;
			[NativeDisableParallelForRestriction]
			public FNativeList<Vector2> uvArray;

			public int AtlasRow;
			public int AtlasModulo;

			public void Execute()
			{
				int count = verticeArray.Length;
				Vector2 uvData = new Vector2(0, 0);

				int offsetposition = 0;
				Vector2 uv = new Vector2(0,0);
				if (atlasDimension.IsCreated)
				{
					uvArray.Clear();
					for (int i = 0; i < count; i++)
					{
						Vector3 vertex = verticeArray[i];
						uvData.x = 0;
						uvData.y = 0;

						Vector3Int inner = NativeVoxelTree.ConvertLocalToInner(vertex, atlasDimension.RootSize);
						int id = atlasDimension._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0, 0);

						Vector2 offset = new Vector2(id, id);

						float uvoffsetx = 1.0f / AtlasRow;
						float uvoffsety = 1.0f / AtlasRow;
						int subdivision = 256 / (AtlasRow * AtlasRow);

						if (i % AtlasModulo == 0)
						{
							offsetposition = (int)offset.x;
						}

						int atlaspos_X = offsetposition / subdivision;//2
						int atlaspos_Y = atlaspos_X / AtlasRow;//1

						atlaspos_X %= AtlasRow;//1

						float uvoffset_X = uvoffsetx * atlaspos_X;

						float uvoffset_Y = uvoffsety * atlaspos_Y;


						uv.x %= 1.0f;
						uv.y %= 1.0f;

						uv.x /= AtlasRow;
						uv.y /= AtlasRow;

						uv.x += uvoffset_X;
						uv.y += uvoffset_Y;

						uvArray.Add(uv);
					}
				}
			}
		}

		internal override ModularUniformVisualHull.WorkType EvaluateWorkType(int dimension)
		{
			if (dimension == AtlasTextureDimension)
				return ModularUniformVisualHull.WorkType.RequiresNonGeometryData;

			return ModularUniformVisualHull.WorkType.Nothing;
		}

		internal override void GetFractionalGeoChecksum(ref ModularUniformVisualHull.FractionalChecksum fractional, SurfaceModifierContainer container)
		{
			float sum = AtlasTextureDimension + AtlasRows + AtlasModulo;
			fractional.nongeometryChecksum += sum + (container.Disabled ? 0 : 1);
		}
	}
}
