using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;
using Fraktalia.Core.Collections;
using Fraktalia.Core.Mathematics;
using Unity.Profiling;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Fraktalia.Utility;

namespace Fraktalia.VoxelGen.Visualisation
{
	public struct VoxelCorner<T> : IEnumerable<T> where T : struct
	{
		public T Corner1;
		public T Corner2;
		public T Corner3;
		public T Corner4;
		public T Corner5;
		public T Corner6;
		public T Corner7;
		public T Corner8;

		/// <summary>
		/// The indexer for the voxel corners
		/// </summary>
		/// <param name="index">The corner's index</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when index is larger than 7.</exception>
		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return Corner1;
					case 1: return Corner2;
					case 2: return Corner3;
					case 3: return Corner4;
					case 4: return Corner5;
					case 5: return Corner6;
					case 6: return Corner7;
					case 7: return Corner8;
					default: throw new ArgumentOutOfRangeException($"There are only 8 corners! You tried to access the corner at index {index}");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						Corner1 = value;
						break;
					case 1:
						Corner2 = value;
						break;
					case 2:
						Corner3 = value;
						break;
					case 3:
						Corner4 = value;
						break;
					case 4:
						Corner5 = value;
						break;
					case 5:
						Corner6 = value;
						break;
					case 6:
						Corner7 = value;
						break;
					case 7:
						Corner8 = value;
						break;
					default: throw new ArgumentOutOfRangeException($"There are only 8 corners! You tried to access the corner at index {index}");
				}
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < 8; i++)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	/// <summary>
	/// A container for a vertex list with 12 vertices
	/// </summary>
	public struct VertexList<T> : IEnumerable<T> where T : struct
	{
		/// <summary>
		/// The first vertex
		/// </summary>
		private T _c1;

		/// <summary>
		/// The second vertex
		/// </summary>
		private T _c2;

		/// <summary>
		/// The third vertex
		/// </summary>
		private T _c3;

		/// <summary>
		/// The fourth vertex
		/// </summary>
		private T _c4;

		/// <summary>
		/// The fifth vertex
		/// </summary>
		private T _c5;

		/// <summary>
		/// The sixth vertex
		/// </summary>
		private T _c6;

		/// <summary>
		/// The seventh vertex
		/// </summary>
		private T _c7;

		/// <summary>
		/// The eighth vertex
		/// </summary>
		private T _c8;

		/// <summary>
		/// The ninth vertex
		/// </summary>
		private T _c9;

		/// <summary>
		/// The tenth vertex
		/// </summary>
		private T _c10;

		/// <summary>
		/// The eleventh vertex
		/// </summary>
		private T _c11;

		/// <summary>
		/// The twelfth vertex
		/// </summary>
		private T _c12;

		/// <summary>
		/// The indexer for the vertex list
		/// </summary>
		/// <param name="index">The vertex's index</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when the index is more than 11.</exception>
		public T this[int index]
		{
			get
			{
				switch (index)
				{
					case 0: return _c1;
					case 1: return _c2;
					case 2: return _c3;
					case 3: return _c4;
					case 4: return _c5;
					case 5: return _c6;
					case 6: return _c7;
					case 7: return _c8;
					case 8: return _c9;
					case 9: return _c10;
					case 10: return _c11;
					case 11: return _c12;
					default: throw new ArgumentOutOfRangeException($"There are only 12 vertices! You tried to access the vertex at index {index}");
				}
			}
			set
			{
				switch (index)
				{
					case 0:
						_c1 = value;
						break;
					case 1:
						_c2 = value;
						break;
					case 2:
						_c3 = value;
						break;
					case 3:
						_c4 = value;
						break;
					case 4:
						_c5 = value;
						break;
					case 5:
						_c6 = value;
						break;
					case 6:
						_c7 = value;
						break;
					case 7:
						_c8 = value;
						break;
					case 8:
						_c9 = value;
						break;
					case 9:
						_c10 = value;
						break;
					case 10:
						_c11 = value;
						break;
					case 11:
						_c12 = value;
						break;
					default: throw new ArgumentOutOfRangeException($"There are only 12 vertices! You tried to access the vertex at index {index}");
				}
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < 12; i++)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	[BurstCompile]
	public struct MarchingCubes_CPU_v2_Calculation : IJob
	{	
		[NativeDisableParallelForRestriction]
		public NativeVoxelTree data;

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

		[ReadOnly]
		public int MergeUV;
	
		public Fint3 positionoffset_Inner;
		public Ffloat3 positionoffset;
		public float voxelSize;
		public float cellSize;

		public float Surface;

		public float minimumID;
		public float maximumID;

		public float UVPower;
		public float SmoothingAngle;
		public int MaxBlocks;

		public int Shrink;

		private int Width;
		private int BorderWidth;
		public int voxelSizeBitPosition;

		public FNativeList<Vector3> verticeArray;
		public FNativeList<int> triangleArray;
		public FNativeList<Vector2> uvArray;

		//UV2 is reserved by Unity for Baked Light
		public FNativeList<Vector2> uv3Array;
		public FNativeList<Vector2> uv4Array;
		public FNativeList<Vector2> uv5Array;
		public FNativeList<Vector2> uv6Array;

		public FNativeList<Vector3> normalArray;
		public FNativeList<Vector4> tangentsArray;
		public FNativeList<Vector3> tan1;
		public FNativeList<Vector3> tan2;
		public FNativeList<Color> colorArray;


		public FNativeMultiHashMap<Vector3, VertexEntry> normalrecalculationDictionary;
		public FNativeList<Vector3> normalrecalculationDictionaryKeys;
		public FNativeList<Vector3> triNormals;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<byte> UniformGrid;


		[BurstDiscard]
		public void Init(int width, int coreIndex)
		{
			
			Width = width;
			BorderWidth = width + 3;
			int blocks = Width * Width * Width;
			MaxBlocks = blocks;

			verticeArray = ContainerStaticLibrary.GetVector3FNativeList("VertexList", coreIndex);
			triangleArray = ContainerStaticLibrary.GetIntFNativeList("IndexList", coreIndex);
			uvArray = ContainerStaticLibrary.GetVector2FNativeList("UVList", coreIndex);
			uv3Array = ContainerStaticLibrary.GetVector2FNativeList("UV3List", coreIndex);
			uv4Array = ContainerStaticLibrary.GetVector2FNativeList("UV4List", coreIndex);
			uv5Array = ContainerStaticLibrary.GetVector2FNativeList("UV5List", coreIndex);
			uv6Array = ContainerStaticLibrary.GetVector2FNativeList("UV6List", coreIndex);
			normalArray = ContainerStaticLibrary.GetVector3FNativeList("NormalList", coreIndex);
			tangentsArray = ContainerStaticLibrary.GetVector4FNativeList("TangentList", coreIndex);
			tan1 = ContainerStaticLibrary.GetVector3FNativeList("Tangent1", coreIndex);
			tan2 = ContainerStaticLibrary.GetVector3FNativeList("Tangent2", coreIndex);
			normalrecalculationDictionary = ContainerStaticLibrary.GetFNativeVector3VertexEntryDict("RecalcNormal", coreIndex);
			normalrecalculationDictionaryKeys = ContainerStaticLibrary.GetVector3FNativeList("RecalcNormalKeys", coreIndex);
			triNormals = ContainerStaticLibrary.GetVector3FNativeList("TriNormalList", coreIndex);
			colorArray = ContainerStaticLibrary.GetColorNativeList("ColorList", coreIndex);
		}

		int VoxelValue(int x, int y, int z)
		{
			int fx = positionoffset_Inner.x + ((x) << voxelSizeBitPosition);
			int fy = positionoffset_Inner.y + ((y) << voxelSizeBitPosition);
			int fz = positionoffset_Inner.z + ((z) << voxelSizeBitPosition);
			return data._PeekVoxelId_InnerCoordinate(fx, fy, fz, 10, Shrink);
		}

		byte VoxelValueFromUniformGrid(int x, int y, int z)
		{
			int index = (z) + (y) * BorderWidth + (x) * BorderWidth * BorderWidth;
			return UniformGrid[index];
		}

		void VoxelValueWithGradient(Fint3 i, int index, out byte voxelID, out Ffloat3 gradientID)
		{
			Fint3 i_n = i - Fint3.one;
			Fint3 i_p = i + Fint3.one;
			byte v = VoxelValueFromUniformGrid(i.x, i.y, i.z);
			byte v_nx = VoxelValueFromUniformGrid(i_n.x, i.y, i.z);
			byte v_px = VoxelValueFromUniformGrid(i_p.x, i.y, i.z);
			byte v_ny = VoxelValueFromUniformGrid(i.x, i_n.y, i.z);
			byte v_py = VoxelValueFromUniformGrid(i.x, i_p.y, i.z);
			byte v_nz = VoxelValueFromUniformGrid(i.x, i.y, i_n.z);
			byte v_pz = VoxelValueFromUniformGrid(i.x, i.y, i_p.z);

			voxelID = v;
			gradientID = new Ffloat3(v_px - v_nx, v_py - v_ny, v_pz - v_nz);
		}

		void FillCube(int x, int y, int z, ref VoxelCorner<byte> Cube, ref VoxelCorner<Ffloat3> CubeNormals)
		{
			byte voxelID;
			Ffloat3 gradientID;
			VoxelValueWithGradient(new Fint3(x, y, z), 0, out voxelID, out gradientID);
			Cube.Corner1 = voxelID;
			CubeNormals.Corner1 = gradientID;
			VoxelValueWithGradient(new Fint3(x + 1, y, z), 1, out voxelID, out gradientID);
			Cube.Corner2 = voxelID;
			CubeNormals.Corner2 = gradientID;
			VoxelValueWithGradient(new Fint3(x + 1, y + 1, z), 2, out voxelID, out gradientID);
			Cube.Corner3 = voxelID;
			CubeNormals.Corner3 = gradientID;
			VoxelValueWithGradient(new Fint3(x, y + 1, z), 3, out voxelID, out gradientID);
			Cube.Corner4 = voxelID;
			CubeNormals.Corner4 = gradientID;
			VoxelValueWithGradient(new Fint3(x, y, z + 1), 4, out voxelID, out gradientID);
			Cube.Corner5 = voxelID;
			CubeNormals.Corner5 = gradientID;
			VoxelValueWithGradient(new Fint3(x + 1, y, z + 1), 5, out voxelID, out gradientID);
			Cube.Corner6 = voxelID;
			CubeNormals.Corner6 = gradientID;
			VoxelValueWithGradient(new Fint3(x + 1, y + 1, z + 1), 6, out voxelID, out gradientID);
			Cube.Corner7 = voxelID;
			CubeNormals.Corner7 = gradientID;
			VoxelValueWithGradient(new Fint3(x, y + 1, z + 1), 7, out voxelID, out gradientID);
			Cube.Corner8 = voxelID;
			CubeNormals.Corner8 = gradientID;
		}

		public void Execute()
		{
			verticeArray.Clear();
			triangleArray.Clear();
			uvArray.Clear();
			uv3Array.Clear();
			uv4Array.Clear();
			uv5Array.Clear();
			uv6Array.Clear();
			tangentsArray.Clear();
			normalArray.Clear();
			tan1.Clear();
			tan2.Clear();
			colorArray.Clear();
			int blocks = MaxBlocks;
			int count = 0;
			int index = 0;

			bool uv3Created = texturedata_UV3.IsCreated;
			bool uv4Created = texturedata_UV4.IsCreated;
			bool uv5Created = texturedata_UV5.IsCreated;
			bool uv6Created = texturedata_UV6.IsCreated;
			bool mergeUV = (MergeUV == 1) ? true : false;


			VoxelCorner<byte> Cube = new VoxelCorner<byte>();
			VoxelCorner<Ffloat3> CubeNormals = new VoxelCorner<Ffloat3>();

			VertexList<Ffloat3> EdgeVertex = new VertexList<Ffloat3>();
			VertexList<Ffloat3> EdgeNormals = new VertexList<Ffloat3>();

			float RootSize = data.RootSize;

			int borderWidthMinusOne = BorderWidth - 1;
			for (int x = -1; x < borderWidthMinusOne; x++)
			{
				for (int y = -1; y < borderWidthMinusOne; y++)
				{
					for (int z = -1; z < borderWidthMinusOne; z++)
					{
						UniformGrid[index] = (byte)VoxelValue(x, y, z);
						index++;
					}
				}
			}

			int vert1, vert2, vert3;
			int i;	
			int cubeIndex;
			float offset;
			Fint3 inner;

			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Width; y++)
				{
					for (int z = 0; z < Width; z++)
					{
						FillCube(x+1, y+1, z+1, ref Cube, ref CubeNormals);
						cubeIndex = CalculateCubeIndex(Cube, (byte)Surface);

						if (cubeIndex == 0 || cubeIndex == 255)
						{
							continue;
						}
						//Find which edges are intersected by the surface
						int edgeFlags = MarchingCubes_Data.ReadOnlyCubeEdgeFlags[cubeIndex];

						//Find the point of intersection of the surface with each edge
						for (i = 0; i < 12; i++)
						{
							//if there is an intersection on this edge
							if ((edgeFlags & (1 << i)) != 0)
							{
								int edgeconnection = MarchingCubes_Data.ReadOnlyEdgeConnection[i * 2 + 0];
								int edgeconnection_end = MarchingCubes_Data.ReadOnlyEdgeConnection[i * 2 + 1];
								offset = GetOffset(Cube[edgeconnection], Cube[edgeconnection_end]);

								Ffloat3 edge;
								int edgeconnection3 = edgeconnection * 3;
								int i3 = i * 3;

								edge.x = x + (MarchingCubes_Data.ReadOnlyVertexOffset[edgeconnection3 + 0] + offset * MarchingCubes_Data.ReadOnlyEdgeDirection[i3 + 0]);
								edge.y = y + (MarchingCubes_Data.ReadOnlyVertexOffset[edgeconnection3 + 1] + offset * MarchingCubes_Data.ReadOnlyEdgeDirection[i3 + 1]);
								edge.z = z + (MarchingCubes_Data.ReadOnlyVertexOffset[edgeconnection3 + 2] + offset * MarchingCubes_Data.ReadOnlyEdgeDirection[i3 + 2]);
								EdgeVertex[i] = edge;

								Ffloat3 sample1 = CubeNormals[edgeconnection];
								Ffloat3 sample2 = CubeNormals[edgeconnection_end];
								EdgeNormals[i] = -Fmath.lerp(sample1, sample2, offset);
							}
						}

						int rowIndex = 16 * cubeIndex;				
						for (i = 0; i < 15; i += 3)
						{
							vert1 = MarchingCubes_Data.ReadOnlyTriangleConnectionTable[rowIndex + i + 0];
							if (vert1 == -1) break;

							vert2 = MarchingCubes_Data.ReadOnlyTriangleConnectionTable[rowIndex + i + 1];
							vert3 = MarchingCubes_Data.ReadOnlyTriangleConnectionTable[rowIndex + i + 2];

							Ffloat3 vertex1 = EdgeVertex[vert1] * voxelSize + positionoffset;
							Ffloat3 vertex2 = EdgeVertex[vert2] * voxelSize + positionoffset;
							Ffloat3 vertex3 = EdgeVertex[vert3] * voxelSize + positionoffset;

							if (!vertex1.Equals(vertex2) && !vertex1.Equals(vertex3) && !vertex2.Equals(vertex3))
							{
								Ffloat3 normal1 = EdgeNormals[vert1];
								Ffloat3 normal2 = EdgeNormals[vert2];
								Ffloat3 normal3 = EdgeNormals[vert3];

								verticeArray.Add(vertex1);
								normalArray.Add(normal1);
								triangleArray.Add(count);
								tan1.Add(new Vector3(0, 0, 0));
								tan2.Add(new Vector3(0, 0, 0));
								inner = NativeVoxelTree.ConvertLocalToInner(vertex1, RootSize);
								FillUVs(mergeUV, uv3Created, uv4Created, uv5Created, uv6Created, inner);
								count++;

								verticeArray.Add(vertex2);
								normalArray.Add(normal2);
								triangleArray.Add(count);
								tan1.Add(new Vector3(0, 0, 0));
								tan2.Add(new Vector3(0, 0, 0));
								inner = NativeVoxelTree.ConvertLocalToInner(vertex2, RootSize);
								FillUVs(mergeUV, uv3Created, uv4Created, uv5Created, uv6Created, inner);
								count++;

								verticeArray.Add(vertex3);
								normalArray.Add(normal3);
								triangleArray.Add(count);
								tan1.Add(new Vector3(0, 0, 0));
								tan2.Add(new Vector3(0, 0, 0));
								inner = NativeVoxelTree.ConvertLocalToInner(vertex3, RootSize);
								FillUVs(mergeUV, uv3Created, uv4Created, uv5Created, uv6Created, inner);
								count++;
							}
						}				
					}
				}
			}
		
			int trianglecount = triangleArray.Length;
			for (i = 0; i < trianglecount; i += 3)
			{
				colorArray.Add(new Color(1, 0, 0));
				colorArray.Add(new Color(0, 1, 0));
				colorArray.Add(new Color(0, 0, 1));
			}

			CalculateCubemap(count);
			ExecuteTangents();	
		}

		private float GetOffset(float v1, float v2)
		{
			float delta = v2 - v1;
			if (delta == 0.0f)
			{
				return Surface;
			}
			return (Surface - v1) / delta;
		}

		public void ExecuteTangents()
		{

			//variable definitions
			int triangleCount = triangleArray.Length;
			int vertexCount = verticeArray.Length;


			for (int a = 0; a < triangleCount; a += 3)
			{
				int i1 = triangleArray[a + 0];
				int i2 = triangleArray[a + 1];
				int i3 = triangleArray[a + 2];

				Vector3 v1 = verticeArray[i1];
				Vector3 v2 = verticeArray[i2];
				Vector3 v3 = verticeArray[i3];

				Vector2 w1 = uvArray[i1];
				Vector2 w2 = uvArray[i2];
				Vector2 w3 = uvArray[i3];

				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;
				float z1 = v2.z - v1.z;
				float z2 = v3.z - v1.z;

				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;

				float div = s1 * t2 - s2 * t1;
				float r = div == 0.0f ? 0.0f : 1.0f / div;

				Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
				Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;

				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;
			}


			for (int a = 0; a < vertexCount; ++a)
			{
				Vector3 n = normalArray[a];
				Vector3 t = tan1[a];

				//Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
				//tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
				Vector3.OrthoNormalize(ref n, ref t);

				Vector4 output = new Vector4();
				output.x = t.x;
				output.y = t.y;
				output.z = t.z;

				output.w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
				tangentsArray.Add(output);

			}


		}

		public void RecalculateNormals(float angle)
		{
			normalrecalculationDictionary.Clear();
			normalrecalculationDictionaryKeys.Clear();
			triNormals.Clear();

			var cosineThreshold = Mathf.Cos(angle * Mathf.Deg2Rad);

			// Holds the normal of each triangle in each sub mesh.
			//var triNormals = new Vector3[triangleArray.Length / 3];

			for (int i = 0; i < triangleArray.Length / 3; i++)
			{
				triNormals.Add(new Vector3(0, 0, 0));
			}


			var triangles = triangleArray;

			for (var i = 0; i < triangles.Length; i += 3)
			{
				int i1 = triangles[i];
				int i2 = triangles[i + 1];
				int i3 = triangles[i + 2];

				// Calculate the normal of the triangle
				Vector3 p1 = verticeArray[i2] - verticeArray[i1];
				Vector3 p2 = verticeArray[i3] - verticeArray[i1];
				Vector3 normal = Vector3.Cross(p1, p2).normalized;
				int triIndex = i / 3;
				triNormals[triIndex] = normal;



				VertexEntry entry;
				FNativeMultiHashMapIterator<Vector3> iter;
				Vector3 hash = verticeArray[i1];
				if (!normalrecalculationDictionary.TryGetFirstValue(hash, out entry, out iter))
				{
					normalrecalculationDictionaryKeys.Add(hash);
				}
				normalrecalculationDictionary.Add(hash, new VertexEntry(triIndex, i1));

				hash = verticeArray[i2];
				if (!normalrecalculationDictionary.TryGetFirstValue(hash, out entry, out iter))
				{
					normalrecalculationDictionaryKeys.Add(hash);
				}
				normalrecalculationDictionary.Add(hash, new VertexEntry(triIndex, i2));

				hash = verticeArray[i3];
				if (!normalrecalculationDictionary.TryGetFirstValue(hash, out entry, out iter))
				{
					normalrecalculationDictionaryKeys.Add(hash);
				}
				normalrecalculationDictionary.Add(hash, new VertexEntry(triIndex, i3));

			}


			// Each entry in the dictionary represents a unique vertex position.
			for (int i = 0; i < normalrecalculationDictionaryKeys.Length; i++)
			{
				FNativeMultiHashMapIterator<Vector3> it_i;
				FNativeMultiHashMapIterator<Vector3> it_j;
				VertexEntry lhsEntry;
				VertexEntry rhsEntry;
				bool hasvalue_I = normalrecalculationDictionary.TryGetFirstValue(normalrecalculationDictionaryKeys[i], out lhsEntry, out it_i);

				while (hasvalue_I)
				{
					bool hasvalue_J = normalrecalculationDictionary.TryGetFirstValue(normalrecalculationDictionaryKeys[i], out rhsEntry, out it_j);
					var sum = new Vector3();
					while (hasvalue_J)
					{
						if (lhsEntry.VertexIndex == rhsEntry.VertexIndex)
						{
							sum += triNormals[rhsEntry.TriangleIndex];
						}
						else
						{

							var dot = Vector3.Dot(
								triNormals[lhsEntry.TriangleIndex],
								triNormals[rhsEntry.TriangleIndex]);
							if (dot >= cosineThreshold)
							{
								sum += triNormals[rhsEntry.TriangleIndex];
							}
						}

						hasvalue_J = normalrecalculationDictionary.TryGetNextValue(out rhsEntry, ref it_j);
					}

					normalArray[lhsEntry.VertexIndex] = sum.normalized;

					hasvalue_I = normalrecalculationDictionary.TryGetNextValue(out lhsEntry, ref it_i);
				}
			}
		}

		public void CalculateCubemap(int count)
		{
			Vector2 output;
			for (int i = 0; i < count; i += 3)
			{
				Vector3 vertex1 = (verticeArray[i]);
				Vector3 vertex2 = (verticeArray[i + 1]);
				Vector3 vertex3 = (verticeArray[i + 2]);
				Vector3 vertex = Vector3.Cross(vertex2 - vertex1, vertex3 - vertex1).normalized;

				float absX = Mathf.Abs(vertex.x);
				float absY = Mathf.Abs(vertex.y);
				float absZ = Mathf.Abs(vertex.z);
				int choosenX = 0;
				int choosenY = 0;
				int choosenZ = 0;
				if (absX > absY && absX > absZ)
				{
					choosenX = 1;
				}
				else if (absY > absZ)
				{
					choosenY = 1;
				}
				else
				{
					choosenZ = 1;
				}


				vertex = vertex1;
				output = new Vector2();
				output += new Vector2(vertex.z, vertex.y) * choosenX;
				output += new Vector2(vertex.x, vertex.z) * choosenY;
				output += new Vector2(vertex.x, vertex.y) * choosenZ;
				Vector2 uv = output * UVPower;
				uvArray.Add(uv);

				vertex = vertex2;
				output = new Vector2();
				output += new Vector2(vertex.z, vertex.y) * choosenX;
				output += new Vector2(vertex.x, vertex.z) * choosenY;
				output += new Vector2(vertex.x, vertex.y) * choosenZ;
				uv = output * UVPower;
				uvArray.Add(uv);

				vertex = vertex3;
				output = new Vector2();
				output += new Vector2(vertex.z, vertex.y) * choosenX;
				output += new Vector2(vertex.x, vertex.z) * choosenY;
				output += new Vector2(vertex.x, vertex.y) * choosenZ;
				uv = output * UVPower;
				uvArray.Add(uv);

				/*
				Vector3 n = (verticeArray[i] - Vector3.one*20).normalized;
				output.x = Mathf.Atan2(n.x, n.z) / (2 * Mathf.PI) + 0.5f;
				output.y = n.y * 0.5f + 0.5f;
				output2.x = Mathf.Atan2(n.y, n.z) / (2 * Mathf.PI) + 0.5f;
				output2.y = n.x * 0.5f + 0.5f;
				output3.x = Mathf.Atan2(n.x, n.y) / (2 * Mathf.PI) + 0.5f;
				output3.y = n.z * 0.5f + 0.5f;

				float u = (Mathf.Atan2(n.z, n.x) / (2f * Mathf.PI));
				float v = (Mathf.Asin(n.y) / Mathf.PI) + 0.5f;

				uvArray.Add(new Vector2(u,v)* UVPower);
				*/
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte CalculateCubeIndex(VoxelCorner<byte> voxelDensities, byte surface)
		{
			int cubeIndex = Fmath.select(0, 1, voxelDensities.Corner1 <= surface);
			cubeIndex |= Fmath.select(0, 2, voxelDensities.Corner2 <= surface);
			cubeIndex |= Fmath.select(0, 4, voxelDensities.Corner3 <= surface);
			cubeIndex |= Fmath.select(0, 8, voxelDensities.Corner4 <= surface);
			cubeIndex |= Fmath.select(0, 16, voxelDensities.Corner5 <= surface);
			cubeIndex |= Fmath.select(0, 32, voxelDensities.Corner6 <= surface);
			cubeIndex |= Fmath.select(0, 64, voxelDensities.Corner7 <= surface);
			cubeIndex |= Fmath.select(0, 128, voxelDensities.Corner8 <= surface);
			return (byte)cubeIndex;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void FillUVs(bool mergeuv, bool uv3Created, bool uv4Created, bool uv5Created, bool uv6Created, Fint3 inner)
		{
			if (mergeuv)
			{			
				if (uv3Created && uv5Created)
				{
					float texturevalue = texturedata_UV3._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0) * TexturePowerUV3;
					float texturevalue2 = texturedata_UV5._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0) * TexturePowerUV5;
					uv3Array.Add(new Vector2(texturevalue, texturevalue2));
				}
				else if (uv3Created)
				{
					float texturevalue = texturedata_UV3._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0) * TexturePowerUV3;
					uv3Array.Add(new Vector2(texturevalue, texturevalue));
				}

				if (uv4Created && uv6Created)
				{
					float texturevalue = texturedata_UV4._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0) * TexturePowerUV4;
					float texturevalue2 = texturedata_UV6._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0) * TexturePowerUV6;
					uv4Array.Add(new Vector2(texturevalue, texturevalue2));
				}
				else if (uv6Created)
				{
					float texturevalue = texturedata_UV4._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0) * TexturePowerUV4;
					uv4Array.Add(new Vector2(texturevalue, texturevalue));
				}
			}
			else
			{			
				if (texturedata_UV3.IsCreated)
				{
					float texturevalue = texturedata_UV3._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0, 0) * TexturePowerUV3;
					uv3Array.Add(new Vector2(texturevalue, texturevalue));
				}

				if (texturedata_UV4.IsCreated)
				{
					float texturevalue = texturedata_UV4._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0, 0) * TexturePowerUV4;
					uv4Array.Add(new Vector2(texturevalue, texturevalue));
				}

				if (texturedata_UV5.IsCreated)
				{
					float texturevalue = texturedata_UV5._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0, 0) * TexturePowerUV5;
					uv5Array.Add(new Vector2(texturevalue, texturevalue));
				}

				if (texturedata_UV6.IsCreated)
				{
					float texturevalue = texturedata_UV6._PeekVoxelId_InnerCoordinate(inner.x, inner.y, inner.z, 10, 0, 0) * TexturePowerUV6;
					uv6Array.Add(new Vector2(texturevalue, texturevalue));
				}
			}
		}
	}

}
