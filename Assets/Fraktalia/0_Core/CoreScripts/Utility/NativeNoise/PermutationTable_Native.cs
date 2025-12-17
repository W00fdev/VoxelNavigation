using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Fraktalia.Utility.NativeNoise
{
	/// <summary>
	/// Random works only in normal C# space. Randomness in Native, Shaders or Compute Shaders require randomness Table.
	/// Pseudo randomness is still randomness. RNGesus truly exists! May the mighty RNG be with you.
	/// </summary>
	public unsafe struct PermutationTable_Native
	{

		public int Size;

		public int Seed;

		public int Max;

		public float Inverse;

		public int Wrap;

		private NativeArray<int> Table;

		[NativeDisableUnsafePtrRestriction]
		public IntPtr unsavePermutationTable;

		public PermutationTable_Native(int size, int max, int seed)
		{
			Size = size;
			Wrap = Size - 1;
			Max = Math.Max(1, max);
			Inverse = 1.0f / Max;

			Seed = seed;
			Table = new NativeArray<int>(Size, Allocator.Persistent);

			System.Random rnd = new System.Random(Seed);

			unsavePermutationTable = (IntPtr)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>() * size, UnsafeUtility.AlignOf<int>(), Allocator.Persistent);
			
			void* ptr = unsavePermutationTable.ToPointer();
			for (int i = 0; i < Size; i++)
			{
				int rng = rnd.Next();
				Table[i] = rng;
				UnsafeUtility.WriteArrayElement<int>(ptr, i, rng);			
			}
		}

		public void CleanUp()
		{
			if (Table.IsCreated)
			{
				Table.Dispose();
				UnsafeUtility.Free(unsavePermutationTable.ToPointer(), Allocator.Persistent);		
			}
		}

		public bool IsCreated
		{
			get
			{
				return Table.IsCreated;
			}
		}

        internal int this[int i]
        {
            get
            {
                return Table[i & Wrap] & Max;
            }
        }

        internal int this[int i, int j]
        {
            get
            {
                return Table[(j + Table[i & Wrap]) & Wrap] & Max;
            }
        }

        internal int this[int i, int j, int k]
        {
            get
            {
                return Table[(k + Table[(j + Table[i & Wrap]) & Wrap]) & Wrap] & Max;
            }
        }
    }

	public struct PermutationCluster
    {
		public int a;
		public int b;
		public int c;
		public int d;
		public int e;
		public int f;
		public int g;
		public int h;
    }
}
