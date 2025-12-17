using Fraktalia.Core.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Fraktalia.VoxelGen
{
	public unsafe struct NativeVoxelReservoir
	{
		public static Dictionary<int, Stack<NativeVoxelReservoir>> UnusedReservoirs = new Dictionary<int, Stack<NativeVoxelReservoir>>();
		public static bool PreserverReservoir = true;

		public int NodeChildrenCount;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<IntPtr> NodePool;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> Information;

		[NativeDisableUnsafePtrRestriction]
		public IntPtr NullAddress;

		public int GarbageSize {
			get
			{
				if (!Information.IsCreated) return 0;
				return Information[0];
			}
			private set
			{
				Information[0] = value;
			}
		}

		public int IsInitialized;
		
		public void Initialize(VoxelGenerator generator)
		{
			NodeChildrenCount = generator.SubdivisionPower * generator.SubdivisionPower * generator.SubdivisionPower;

			if (PreserverReservoir)
            {
				if(UnusedReservoirs.TryGetValue(NodeChildrenCount, out var result))
                {
					if(result.TryPop(out var pop))
                    {
						NodePool = pop.NodePool;
						Information = pop.Information;
						NullAddress = pop.NullAddress;
						IsInitialized = 1;
						return;
                    }
                }
            }
			
			NodePool = new FNativeList<IntPtr>(Allocator.Persistent);
			Information = new NativeArray<int>(2, Allocator.Persistent);	
			GarbageSize = 0;

			allocateNullNode();
			allocateNodes(1000);

			IsInitialized = 1;
		}

		public IntPtr ObtainNodeAddress()
		{
			if (GarbageSize == 0)
			{
				allocateNodes(100);
			}
			GarbageSize--;
			IntPtr address = NodePool[GarbageSize];
			
			return address;
		}

		public void AddGarbage(IntPtr address)
		{
			if (GarbageSize >= NodePool.Length)
			{
				NodePool.Add(address);
			}
			else
			{
				NodePool[GarbageSize] = address;
			}

			GarbageSize++;
		}

		public void CleanUp()
		{
			if (IsInitialized == 1)
			{
				IsInitialized = 0;

				if (PreserverReservoir)
				{
					if (!UnusedReservoirs.ContainsKey(NodeChildrenCount)) UnusedReservoirs[NodeChildrenCount] = new Stack<NativeVoxelReservoir>();
					UnusedReservoirs[NodeChildrenCount].Push(this);
					return;
				}

				disposeReservoir();
			}
		}

		public static void CleanReservoirs()
        {
            foreach (var item in UnusedReservoirs)
            {
                while (item.Value.TryPop(out var reservoir))
                {
					reservoir.disposeReservoir();
				}
            }
        }


		private void disposeReservoir()
        {
			FreeMemory();
			NodePool.Dispose();
			Information.Dispose();
			UnsafeUtility.Free(NullAddress.ToPointer(), Allocator.Persistent);
		}

		private void allocateNodes(int amount)
		{
			int garbagesize = GarbageSize;
			for (int i = 0; i < amount; i++)
			{
				int length = NodeChildrenCount;
				int elementSize = UnsafeUtility.SizeOf<NativeVoxelNode>();
				
				IntPtr address = (IntPtr)UnsafeUtility.Malloc(length * elementSize, UnsafeUtility.AlignOf<NativeVoxelNode>(), Allocator.Persistent);
				if (garbagesize >= NodePool.Length)
				{
					NodePool.Add(address);
				}
				else
				{
					NodePool[garbagesize] = address;
				}
				garbagesize++;				
			}
			GarbageSize = garbagesize;
		}

		private void allocateNullNode()
        {
			int length = NodeChildrenCount;
			int elementSize = UnsafeUtility.SizeOf<NativeVoxelNode>();
			NullAddress = (IntPtr)UnsafeUtility.Malloc(length * elementSize, UnsafeUtility.AlignOf<NativeVoxelNode>(), Allocator.Persistent);
			
			for (int i = 0; i < length; i++)
			{
				IntPtr location = IntPtr.Add(NullAddress, i * elementSize);
				NativeVoxelNode newVoxel = new NativeVoxelNode(NullAddress);
				UnsafeUtility.CopyStructureToPtr(ref newVoxel, location.ToPointer());
			}
		}

		public void FreeMemory()
		{
			for (int i = 0; i < GarbageSize; i++)
			{			
				IntPtr pointer = NodePool[i];

#if ENABLE_UNITY_COLLECTIONS_CHECKS
				if (pointer.ToPointer() == null || pointer == IntPtr.Zero)
				{
					Debug.LogError("Something terrible went wrong inside the NativeVoxelReservoir!");
					throw new NullReferenceException();
				}
#endif

				UnsafeUtility.Free(pointer.ToPointer(), Allocator.Persistent);
				
			}

			GarbageSize = 0;
			NodePool.Clear();
		}

		/// <summary>
		/// Clears Memory and sets capacity of NodePool to minimum.
		/// </summary>
		public void ResetGarbage()
		{
			int garbagecount = GarbageSize;
			if(garbagecount > VoxelGenerator.MINCAPACITY)
			{
				FreeMemory();
				NodePool.Capacity = VoxelGenerator.MINCAPACITY;
			}
		}	
	}
}
