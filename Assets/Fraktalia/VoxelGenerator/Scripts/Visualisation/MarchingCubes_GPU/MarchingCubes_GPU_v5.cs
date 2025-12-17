using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System;
using Unity.Burst;
using UnityEngine.Rendering;
using Fraktalia.Core.FraktaliaAttributes;
using Fraktalia.Utility;
using Unity.Collections.LowLevel.Unsafe;
using Fraktalia.Core.Math;
using System.Reflection;
using Fraktalia.Core.Collections;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;
#endif


namespace Fraktalia.VoxelGen.Visualisation
{
	public unsafe class MarchingCubes_GPU_v5 : UniformGridVisualHull_SingleStep_Async
	{
		[BeginInfo("MarchingCubes_GPUv2")]
		[InfoTitle("GPU based marching cubes", "This is the fastest hull generator currently possible. It combines the power of CPU and GPU to increase perfomance far beyond " +
		"CPU only based hull generators. " +
			"\n\n V2 is about 10x faster than the previous one.", "MarchingCubes_GPUv2")]
		[InfoSection1("How to use:", "Functionality is same as the CPU based marching cubes. It is important to have the compute shaders (Marching Cubes and Clear Buffer) assigned. " +
			"\n\nIt is recommended to use a triplanar shader so expensive UV, Normals and Tangents calculations can be avoided. If you cannot use triplanar shaders, you can enable them.", "MarchingCubes_GPUv2")]
		[InfoSection2("Compatibility:", "" +
		"<b>Direct X 11:</b> This hull generator uses Compute Shader which may not be supported by your target system.\n" +
		"For more information check the official statement from Unity:\n\nhttps://docs.unity3d.com/Manual/class-ComputeShader.html\n", "MarchingCubes_GPUv2")]
		[InfoTextKey("GPU based marching cubes:", "MarchingCubes_GPUv2")]
		
		[Range(0, 10)]
		public int SurfaceDimension = 0;
		[Range(0, 255)] [Tooltip("The ID value at which voxel is considered solid.")]
		public int SurfacePoint = 128;
		[Space]
		public int TargetLOD;

		[PropertyKey("Triplanar/UV Mapping Settings:")]	
		[Tooltip("Write barycentric colors into the mesh. Useful for Wireframe or seamless tesellation.")]
		public bool AddBarycentricColors;
		[Tooltip("Create Cube UVs for non-triplanar shader usage.")]
		public bool GenerateCubeUV;
		[Tooltip("Attempt to create spherical UV for planets (experimental).")]
		public bool GenerateSphereUV;
		[Tooltip("Recalculate Normals based on the mesh using Normal Angle for flat or smooth normals.")]
		[LabelText("Recalculate Normals (expensive, super time loss)")]
		public bool RecalculateNormals;
		public float NormalAngle;
		public float UVPower = 1;

		[PropertyKey("Texture Settings", false)]
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

		[PropertyKey("Advanced", false)]
		[Tooltip("Option to reduce outputbuffer size. Increases GPUAsyncReadback performance as the result buffer is smaller but can cause errors when values as < 1.")]
		[Range(0.3f, 1)]
		public float BuffersizeMultiplier = 1;

		[HideInInspector]
		public ComputeShader m_marchingCubes;

		NativeCreateUniformGrid_V2[] calculators;

		public NativeArray<float>[] UniformGrid;

		//The size of the buffer that holds the verts.
		//This is the maximum number of verts that the 
		//marching cube can produce, 5 triangles for each voxel.
		private int SIZE;

		ComputeBuffer[] voxeldata;	
		ComputeBuffer _vertexnormalCombinedBuffer;


		[NativeDisableContainerSafetyRestriction]
		private NativeArray<Vector3> verticesnormalsCombined;
	
		[NativeDisableContainerSafetyRestriction]
		private NativeArray<int> indexVerts;

		private IntPtr[] verticePtrNative;
		private IntPtr[] normalPtrNative;
	
		ComputeBuffer _triangleTable;
		ComputeBuffer[] _counterBuffer;
		ComputeBuffer[] _counterBufferResult;
		ComputeBuffer[] _positionOffsetBuffer;
		Vector3[][] positionoffset;

		GPUToMesh_v2[] dataconverter;
		int BorderedWidth;

		
		private List<int> LODSize;
		private int[][] counter;
		private NativeArray<int>[] ncounter;

		private JobHandle[] dataconverterhandles;
		private JobHandle voxeltogpuhandle;

		private int[] currentWorkTable;

		protected override void Initialize()
		{
			NumCores = Mathf.Clamp(NumCores, 1, 8);
			width = Mathf.ClosestPowerOfTwo(width);
			Cell_Subdivision = Mathf.ClosestPowerOfTwo(Cell_Subdivision);
			if (width < 4) width = 8;
			base.Initialize();			
		}


		protected override void initializeCalculators()
		{
			m_marchingCubes = Resources.Load<ComputeShader>("Voxelica_MarchingCubes_v5");
			
			LODSize = new List<int>();

			int lodsize = width;
			while (lodsize >= 4)
			{
				LODSize.Add(lodsize);
				lodsize = lodsize / 2;
			}

			float fsize = width * width * width * 3 * 5 * BuffersizeMultiplier;
			SIZE = (int)fsize;
			BorderedWidth = width + 3;
		
			if(dataconverter != null)
            {
                for (int i = 0; i < dataconverter.Length; i++)
                {
					dataconverter[i].CleanUp();
                }
            }

			indexVerts = ContainerStaticLibrary.GetRisingNativeIntArray("Index", SIZE);
			UniformGrid = new NativeArray<float>[NumCores];		
			calculators = new NativeCreateUniformGrid_V2[NumCores];
			dataconverter = new GPUToMesh_v2[NumCores];
			counter = new int[NumCores][];
			ncounter = new NativeArray<int>[NumCores];
		
			voxeldata = new ComputeBuffer[NumCores];
			_counterBuffer = new ComputeBuffer[NumCores];
			_counterBufferResult = new ComputeBuffer[NumCores];
			_positionOffsetBuffer = new ComputeBuffer[NumCores];
			positionoffset = new Vector3[NumCores][];

			dataconverterhandles = new JobHandle[NumCores];
			currentWorkTable = new int[NumCores];

			verticePtrNative = new IntPtr[NumCores];		
			normalPtrNative = new IntPtr[NumCores];
		
			int totalbuffersize = (SIZE * 2 * NumCores) + NumCores;
			verticesnormalsCombined = ContainerStaticLibrary.GetVertexArray(totalbuffersize);
			_vertexnormalCombinedBuffer = ContainerStaticLibrary.GetComputeBuffer("NormalVertexBuffer_B" + NumCores, totalbuffersize, sizeof(float) * 3);

			for (int i = 0; i < NumCores; i++)
            {
				int vertexoffset = (SIZE * 2 * i);
				verticePtrNative[i] = IntPtr.Add((IntPtr)verticesnormalsCombined.GetUnsafePtr(), vertexoffset * sizeof(Vector3));
				int normaloffset = (SIZE + SIZE * 2 * i);
				normalPtrNative[i] = IntPtr.Add((IntPtr)verticesnormalsCombined.GetUnsafePtr(), normaloffset * sizeof(Vector3));
				

				counter[i] = new int[1];
				positionoffset[i] = new Vector3[1];

				UniformGrid[i] = ContainerStaticLibrary.GetArray_float(BorderedWidth * BorderedWidth * BorderedWidth, i);
				
				

				calculators[i] = new NativeCreateUniformGrid_V2();
				calculators[i].Width = width;
				if (SurfaceDimension < engine.Data.Length)
				{
					calculators[i].data = engine.Data[SurfaceDimension];
				}
				else
				{
					calculators[i].data = engine.Data[0];
				}
				calculators[i].UniformGridResult = UniformGrid[i];

				dataconverter[i] = new GPUToMesh_v2();
				
				dataconverter[i].managedTriangleArray = (IntPtr)indexVerts.GetUnsafePtr();
				

			
				dataconverter[i].Init();
				if (TextureDimensionUV3 != -1 && TextureDimensionUV3 < engine.Data.Length)
				{
					dataconverter[i].texturedata_UV3 = engine.Data[TextureDimensionUV3];
				}

				if (TextureDimensionUV4 != -1 && TextureDimensionUV4 < engine.Data.Length)
				{
					dataconverter[i].texturedata_UV4 = engine.Data[TextureDimensionUV4];
				}

				if (TextureDimensionUV5 != -1 && TextureDimensionUV5 < engine.Data.Length)
				{
					dataconverter[i].texturedata_UV5 = engine.Data[TextureDimensionUV5];
				}

				if (TextureDimensionUV6 != -1 && TextureDimensionUV6 < engine.Data.Length)
				{
					dataconverter[i].texturedata_UV6 = engine.Data[TextureDimensionUV6];
				}
				dataconverter[i].verts = ContainerStaticLibrary.GetVertexBank("MARCHINGCUBES_GPU", SIZE, i, NumCores);
				ncounter[i] = ContainerStaticLibrary.GetArray_int(1, i);

				
						
				voxeldata[i] = ContainerStaticLibrary.GetComputeBuffer("VoxelBuffer_"+i, BorderedWidth * BorderedWidth * BorderedWidth, sizeof(float));
				voxeldata[i].IsValid();
				_counterBuffer[i] = ContainerStaticLibrary.GetComputeBuffer("CounterBuffer_"+i, 1, 4, ComputeBufferType.Counter);
				_counterBufferResult[i] = ContainerStaticLibrary.GetComputeBuffer("CounterBufferResult_"+i, 1, sizeof(int));
				_positionOffsetBuffer[i] = ContainerStaticLibrary.GetComputeBuffer("PositionOffsetBuffer_" + i, 1, sizeof(float) * 3);

				
			}			

			// Marching cubes triangle table
			_triangleTable = ContainerStaticLibrary.GetComputeBuffer("TriangleTable", 256, sizeof(ulong));
			_triangleTable.SetData(MarchingCubesTables.PaulBourkeTriangleTable);		
		}

		protected override IEnumerator beginCalculationasync(Queue<int> WorkerQueue, float cellSize, float voxelSize)
		{
			int works = 0;
			
			int lod = Mathf.Clamp(TargetLOD, 0, LODSize.Count - 1);
			voxelSize = voxelSize * Mathf.Pow(2, lod);
			int lodwidth = LODSize[lod];
			
			BorderedWidth = lodwidth + 3;
			int LODLength = BorderedWidth * BorderedWidth * BorderedWidth;
			int innerVoxelSize = NativeVoxelTree.ConvertLocalToInner(voxelSize, engine.RootSize);

			
			for (int cores = 0; cores < calculators.Length; cores++)
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
				calculators[cores].positionoffset = offset;
				calculators[cores].Width = BorderedWidth;
				calculators[cores].Shrink = (int)Shrink;
				calculators[cores].voxelSizeBitPosition = MathUtilities.RightmostBitPosition(innerVoxelSize);
				voxeltogpuhandle = calculators[cores].Schedule(LODLength, LODLength / SystemInfo.processorCount, voxeltogpuhandle);

				positionoffset[cores][0] = new Vector3(startX, startY, startZ);
				_positionOffsetBuffer[cores].SetData(positionoffset[cores]);
				
				currentWorkTable[cores] = cellindex;
			}

			int synchronity = 0;
			while (!voxeltogpuhandle.IsCompleted)
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
			voxeltogpuhandle.Complete();

			for (int coreindex = 0; coreindex < works; coreindex++)
			{
				voxeldata[coreindex].SetData(UniformGrid[coreindex]);
				counter[coreindex][0] = 0;
				_counterBufferResult[coreindex].SetData(counter[coreindex]);
				m_marchingCubes.SetInt("_BlockWidth", BorderedWidth);
				m_marchingCubes.SetFloat("_VoxelSize", voxelSize);				
			}

			for (int coreindex = 0; coreindex < works; coreindex++)
			{
				RunCompute(voxeldata[coreindex], SurfacePoint, lodwidth, coreindex);
			}

			AsyncGPUReadbackRequest requestverts = AsyncGPUReadback.RequestIntoNativeArray(ref verticesnormalsCombined, _vertexnormalCombinedBuffer);
			while (!requestverts.done)
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
			requestverts.WaitForCompletion();


			for (int coreindex = 0; coreindex < works; coreindex++)
			{
				Vector3 counts;

				dataconverter[coreindex].managedNormalArray = normalPtrNative[coreindex];
				dataconverter[coreindex].managedVertexArray = verticePtrNative[coreindex];
				counts = verticesnormalsCombined[(SIZE * 2 * NumCores)+coreindex];



				int verticecount = (int)counts.x;
				int meshsize = (Mathf.Clamp(verticecount, 0, verticecount)) * 3;

				dataconverter[coreindex].meshSize = meshsize;
				dataconverter[coreindex].meshSize_triangles = meshsize;
				dataconverter[coreindex].Shrink = Shrink;
				dataconverter[coreindex].UVPower = UVPower;
				dataconverter[coreindex].GenerateCubeUV = GenerateCubeUV ? 1 : 0;
				dataconverter[coreindex].GenerateSphereUV = GenerateSphereUV ? 1 : 0;
				dataconverter[coreindex].rootSize = engine.RootSize;
				dataconverter[coreindex].TexturePowerUV3 = TexturePowerUV3;
				dataconverter[coreindex].TexturePowerUV4 = TexturePowerUV4;
				dataconverter[coreindex].TexturePowerUV5 = TexturePowerUV5;
				dataconverter[coreindex].TexturePowerUV6 = TexturePowerUV6;
				dataconverter[coreindex].MergeUV = MergeUVs ? 1 : 0;
				dataconverter[coreindex].NormalAngle = NormalAngle;
				dataconverter[coreindex].CalculateBarycentricColors = AddBarycentricColors ? 1 : 0;
				dataconverter[coreindex].CalculateNormals = RecalculateNormals ? 1 : 0;

				dataconverterhandles[coreindex] = dataconverter[coreindex].Schedule();
			}

			for (int coreindex = 0; coreindex < works; coreindex++)
			{
				while (!dataconverterhandles[coreindex].IsCompleted)
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
				dataconverterhandles[coreindex].Complete();

				int cellindex = currentWorkTable[coreindex];
				Haschset[cellindex] = false;
				VoxelPiece piece = VoxelMeshes[cellindex];
				if(piece == null)
                {
					continue;
				}

				piece.Clear();

				FNativeList<Vector3> vertices;
				FNativeList<int> triangles;
				FNativeList<Vector3> normals;
                           
				finishCalculation(coreindex, piece, out vertices, out triangles, out normals);
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

		void RunCompute(ComputeBuffer voxelsdata, float target, int lodwidth, int coreindex)
		{
			_counterBuffer[coreindex].SetCounterValue(0);
			//Hull generation
			m_marchingCubes.SetFloat("_Target", target);
			m_marchingCubes.SetInt("_Size", SIZE);
			m_marchingCubes.SetInt("_CoreIndex", coreindex);
			m_marchingCubes.SetInt("_NumCores", NumCores);
			m_marchingCubes.SetInt("_TotalBufferSize", verticesnormalsCombined.Length);


			
			m_marchingCubes.SetBuffer(0, "counterBuffer", _counterBufferResult[coreindex]);
			m_marchingCubes.SetBuffer(0, "TriangleTable", _triangleTable);
			m_marchingCubes.SetBuffer(0, "Voxels", voxelsdata);
			m_marchingCubes.SetBuffer(0, "_VertexNormalBuffer", _vertexnormalCombinedBuffer);
			m_marchingCubes.SetBuffer(0, "Counter", _counterBuffer[coreindex]);
			m_marchingCubes.SetBuffer(0, "PositionOffset", _positionOffsetBuffer[coreindex]);
			m_marchingCubes.Dispatch(0, lodwidth / 4, lodwidth / 4, lodwidth / 4);

			//Finalize the buffer count.
			m_marchingCubes.SetBuffer(1, "_VertexNormalBuffer", _vertexnormalCombinedBuffer);
			m_marchingCubes.SetBuffer(1, "counterBuffer", _counterBufferResult[coreindex]);
			m_marchingCubes.SetBuffer(1, "Counter", _counterBuffer[coreindex]);
			m_marchingCubes.Dispatch(1, 1, 1, 1);
		}
		
		protected override void finishCalculation(int coreindex, VoxelPiece piece, out FNativeList<Vector3> vertices, out FNativeList<int> triangles, out FNativeList<Vector3> normals)
		{
			var usedcalculatorv = dataconverter[coreindex];
			if (usedcalculatorv.verticeArray.Length != 0)
			{
				piece.SetVertices(usedcalculatorv.verticeArray);
				piece.SetTriangles(usedcalculatorv.triangleArray);
				piece.SetNormals(usedcalculatorv.normalArray);
				piece.SetTangents(usedcalculatorv.tangentsArray);
				piece.SetUVs(0, usedcalculatorv.uvArray);

				if (TextureDimensionUV3 != -1)
					piece.SetUVs(2, usedcalculatorv.uv3Array);

				if (TextureDimensionUV4 != -1)
					piece.SetUVs(3, usedcalculatorv.uv4Array);

				if (TextureDimensionUV5 != -1)
					piece.SetUVs(4, usedcalculatorv.uv5Array);

				if (TextureDimensionUV6 != -1)
					piece.SetUVs(5, usedcalculatorv.uv6Array);

				if (GenerateCubeUV)
				{
					piece.SetUVs(0, usedcalculatorv.uvArray);
					piece.SetTangents(usedcalculatorv.tangentsArray);
				}

				if (AddBarycentricColors) piece.SetColors(usedcalculatorv.colorArray);
			}

			vertices = dataconverter[coreindex].verticeArray;
			triangles = dataconverter[coreindex].triangleArray;
			normals = dataconverter[coreindex].normalArray;
		}




		protected override void cleanUpCalculation()
		{
			voxeltogpuhandle.Complete();
			if (dataconverterhandles != null)
			{
				for (int i = 0; i < dataconverterhandles.Length; i++)
				{
					dataconverterhandles[i].Complete();

					if (dataconverter != null)
						dataconverter[i].CleanUp();
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

			return base.GetChecksum() + Cell_Subdivision * 1000 + width * 100 * 10 + SurfacePoint + SurfaceDimension
				+ details + Shrink + TexturePowerUV3 + TexturePowerUV4 + TexturePowerUV5 + TexturePowerUV6 + details + (GenerateCubeUV ? 0 : 1) + (GenerateSphereUV ? 0 : 1) +
				(AddBarycentricColors ? 0 : 1) + (RecalculateNormals ? 0 : 1) + NormalAngle + UVPower + TextureDimensionUV3 + TextureDimensionUV4 + TextureDimensionUV5 + TextureDimensionUV6;
		}

		public override void UpdateLOD(int newLOD)
		{
			base.UpdateLOD(newLOD);
			if (TargetLOD != newLOD)
			{
				TargetLOD = newLOD;
				engine.Rebuild();
			}
		}
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(MarchingCubes_GPU_v5))]
	[CanEditMultipleObjects]
	public class MarchingCubes_GPU_v5Editor : Editor
	{
		private InfoTextKeyAttribute infotext;

		public override void OnInspectorGUI()
		{
			GUIStyle title = new GUIStyle();
			title.fontStyle = FontStyle.Bold;
			title.fontSize = 16;
			title.richText = true;
			title.alignment = TextAnchor.MiddleLeft;

			GUIStyle bold = new GUIStyle();
			bold.fontStyle = FontStyle.Bold;
			bold.fontSize = 12;
			bold.richText = true;

			MarchingCubes_GPU_v5 mytarget = target as MarchingCubes_GPU_v5;

			SerializedProperty prop = serializedObject.GetIterator();

			bool isfoldout = true;
			int verticalgroups = 0;
			
			if(infotext != null)
            {
				EditorGUILayout.BeginVertical();
				Rect position = EditorGUILayout.BeginHorizontal(GUILayout.Height(50));

				if(FraktaliaEditorStyles.InfoTitle(position, infotext.labeltext))
                {
					InfoContent tutorial = BeginInfoAttribute.InfoContentDictionary[infotext.Key];
					TutorialWindow.Init(tutorial);
				}

				EditorGUILayout.Space();
			
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndVertical();
			}

			if (prop.NextVisible(true))
			{
				do
				{
					if (prop.name == "m_Script") continue;

					if (infotext == null)
					{
						infotext = FraktaliaEditorUtility.GetAttribute<InfoTextKeyAttribute>(prop, true);
					}



					PropertyKeyAttribute fold = FraktaliaEditorUtility.GetAttribute<PropertyKeyAttribute>(prop, true);
					if (fold != null)
					{
						if (isfoldout)
						{
							if(verticalgroups > 0)
                            {
								verticalgroups--;
								EditorGUILayout.EndVertical();
							}
							EditorGUI.indentLevel = 0;
						}
						
						verticalgroups++;
						EditorGUILayout.BeginVertical();
						EditorGUILayout.BeginHorizontal();
						isfoldout = EditorGUILayout.Foldout(fold.State, fold.Key, true);
						EditorGUILayout.EndHorizontal();
						fold.State = isfoldout;

						if (!isfoldout)						
                        {
							verticalgroups--;
							EditorGUILayout.EndVertical();							
						}
                        else
                        {
							EditorGUI.indentLevel = 1;
                        }
					}

					if (isfoldout)
					{
						EditorGUILayout.PropertyField(prop, true);
						if (prop.name.Equals("NumCores"))
						{
							EditorGUILayout.BeginHorizontal();
							EditorGUILayout.LabelField("Required GPU cores: " + mytarget.NumCores * 64, bold);
							EditorGUILayout.LabelField("Cell count: " + Mathf.Pow(mytarget.Cell_Subdivision, 3), bold);
							EditorGUILayout.EndHorizontal();
						}						
					}
				}
				while (prop.NextVisible(false));
			}

			if(verticalgroups > 0)
            {
				verticalgroups--;
				EditorGUILayout.EndVertical();
			}

			if (GUI.changed)
			{
				serializedObject.ApplyModifiedProperties();
				EditorUtility.SetDirty(target);
			}



		}

		
	}


#endif
}
