using System.Collections;
using UnityEngine;
using Unity.Collections;
using System;
using UnityEngine.Rendering;
using Fraktalia.Core.FraktaliaAttributes;
using Fraktalia.Utility;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Fraktalia.Core.Math;
using Fraktalia.Core.Mathematics;
using Fraktalia.Core.Collections;
using System.Runtime.CompilerServices;
using Unity.Burst;


#if UNITY_EDITOR
#endif

namespace Fraktalia.VoxelGen.Visualisation
{
    [System.Serializable]
	public unsafe class Module_MarchingCubes_CPU : HullGenerationModule
	{		
		[Range(0, 255)]
		[Tooltip("The ID value at which voxel is considered solid.")]
		public int SurfacePoint = 128;
             
        Module_MarchingCubes_CPU_Calculation[] calculators_v2;
        JobHandle[] jobhandles;
        private int[] currentWorkTable;

        private int BorderedWidth;
		private int NumCores;
		private VoxelGenerator engine;
		private int SIZE;
        private int width;
		protected override void initializeModule()
		{
			width = HullGenerator.width;
			NumCores = HullGenerator.CurrentNumCores;
			engine = HullGenerator.engine;
		
			SIZE = width * width * width * 3 * 5;
			BorderedWidth = width + 3;

            calculators_v2 = new Module_MarchingCubes_CPU_Calculation[NumCores];
            jobhandles = new JobHandle[NumCores];
            currentWorkTable = new int[NumCores];
            for (int i = 0; i < calculators_v2.Length; i++)
            {
                calculators_v2[i].Init(width, i);
                calculators_v2[i].Shrink = HullGenerator.Shrink;          
                calculators_v2[i].data = engine.Data[0];                     
            }
        }

		public override IEnumerator beginCalculationasync(float cellSize, float voxelSize)
		{ 
            voxelSize = voxelSize * Mathf.Pow(2, HullGenerator.CurrentLOD);
			int lodwidth = HullGenerator.LODSize[HullGenerator.CurrentLOD];
			SIZE = lodwidth * lodwidth * lodwidth * 3 * 5;
			BorderedWidth = lodwidth + 3;
			int LODLength = BorderedWidth * BorderedWidth * BorderedWidth;
			int innerVoxelSize = NativeVoxelTree.ConvertLocalToInner(voxelSize, engine.RootSize);
            int Cell_Subdivision = HullGenerator.Cell_Subdivision;

            for (int coreindex = 0; coreindex < HullGenerator.activeCores; coreindex++)
			{
				int cellindex = HullGenerator.activeCells[coreindex];
				if (HullGenerator.WorkInformations[cellindex].CurrentWorktype != ModularUniformVisualHull.WorkType.GenerateGeometry) continue;

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
              
                calculators_v2[coreindex].voxelSizeBitPosition = MathUtilities.RightmostBitPosition(innerVoxelSize);
                calculators_v2[coreindex].voxelSize = cellSize / (lodwidth);
                calculators_v2[coreindex].cellSize = cellSize;
                calculators_v2[coreindex].positionoffset = new Vector3(startX, startY, startZ);
                calculators_v2[coreindex].positionoffset_Inner = new Fint3(offset.x, offset.y, offset.z);               
                calculators_v2[coreindex].Surface = SurfacePoint;
                calculators_v2[coreindex].UniformGrid = HullGenerator.UniformGrid[coreindex];
                calculators_v2[coreindex].Width = lodwidth;
                calculators_v2[coreindex].BorderWidth = BorderedWidth;

                jobhandles[coreindex] = calculators_v2[coreindex].Schedule();
                currentWorkTable[coreindex] = cellindex;
            }

			for (int coreindex = 0; coreindex < HullGenerator.activeCores; coreindex++)
			{
				int cellIndex = HullGenerator.activeCells[coreindex];
				if (HullGenerator.WorkInformations[cellIndex].CurrentWorktype != ModularUniformVisualHull.WorkType.GenerateGeometry) continue;
			
                while (!jobhandles[coreindex].IsCompleted)
                {
                    if (HullGenerator.synchronitylevel < 0) break;
                    yield return new YieldInstruction();
                }
                jobhandles[coreindex].Complete();

                var usedcalculatorv2 = calculators_v2[coreindex];            
				NativeMeshData data = HullGenerator.nativeMeshData[cellIndex];
				data.verticeArray_original.AddRange(usedcalculatorv2.verticeArray);
				data.normalArray_original.AddRange(usedcalculatorv2.normalArray);
				data.triangleArray_original.AddRange(usedcalculatorv2.triangleArray);			
			}

			yield return null;
		}

        public override void CleanUp()
        {
            if (calculators_v2 != null)
            {
                for (int i = 0; i < calculators_v2.Length; i++)
                {
                    jobhandles[i].Complete();
                }
            }
        }

        public override float GetChecksum()
		{
			return SurfacePoint;
		}
	}

    [BurstCompile]
    public struct Module_MarchingCubes_CPU_Calculation : IJob
    {
        [NativeDisableParallelForRestriction]
        public NativeVoxelTree data;

        public Fint3 positionoffset_Inner;
        public Ffloat3 positionoffset;
        public float voxelSize;
        public float cellSize;

        public float Surface;
        public int MaxBlocks;
        public int Shrink;
        public int Width;
        public int BorderWidth;
        public int voxelSizeBitPosition;

        public FNativeList<Vector3> verticeArray;
        public FNativeList<int> triangleArray;
        public FNativeList<Vector3> normalArray;

        public FNativeMultiHashMap<Vector3, VertexEntry> normalrecalculationDictionary;
        public FNativeList<Vector3> normalrecalculationDictionaryKeys;
        public FNativeList<Vector3> triNormals;

        [ReadOnly]
        public NativeArray<float> UniformGrid;


        [BurstDiscard]
        public void Init(int width, int coreIndex)
        {

            Width = width;
            BorderWidth = width + 3;
            int blocks = Width * Width * Width;
            MaxBlocks = blocks;

            verticeArray = ContainerStaticLibrary.GetVector3FNativeList("VertexList", coreIndex);
            triangleArray = ContainerStaticLibrary.GetIntFNativeList("IndexList", coreIndex);
           
            normalArray = ContainerStaticLibrary.GetVector3FNativeList("NormalList", coreIndex);           
            normalrecalculationDictionary = ContainerStaticLibrary.GetFNativeVector3VertexEntryDict("RecalcNormal", coreIndex);
            normalrecalculationDictionaryKeys = ContainerStaticLibrary.GetVector3FNativeList("RecalcNormalKeys", coreIndex);
            triNormals = ContainerStaticLibrary.GetVector3FNativeList("TriNormalList", coreIndex);
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
            int index = (x) + (y) * BorderWidth + (z) * BorderWidth * BorderWidth;
            return (byte)UniformGrid[index];
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
            normalArray.Clear();      
            int blocks = MaxBlocks;
            int count = 0;
            
            VoxelCorner<byte> Cube = new VoxelCorner<byte>();
            VoxelCorner<Ffloat3> CubeNormals = new VoxelCorner<Ffloat3>();

            VertexList<Ffloat3> EdgeVertex = new VertexList<Ffloat3>();
            VertexList<Ffloat3> EdgeNormals = new VertexList<Ffloat3>();

            float RootSize = data.RootSize;       

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
                        FillCube(x + 1, y + 1, z + 1, ref Cube, ref CubeNormals);
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
                                
                                inner = NativeVoxelTree.ConvertLocalToInner(vertex1, RootSize);
                               
                                count++;

                                verticeArray.Add(vertex2);
                                normalArray.Add(normal2);
                                triangleArray.Add(count);
                               
                                inner = NativeVoxelTree.ConvertLocalToInner(vertex2, RootSize);
                                
                                count++;

                                verticeArray.Add(vertex3);
                                normalArray.Add(normal3);
                                triangleArray.Add(count);
                                
                                inner = NativeVoxelTree.ConvertLocalToInner(vertex3, RootSize);                             
                                count++;
                            }
                        }
                    }
                }
            }                   
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
    }
}