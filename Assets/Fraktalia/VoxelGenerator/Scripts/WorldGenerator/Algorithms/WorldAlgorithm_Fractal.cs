using Fraktalia.Core.Mathematics;
using Fraktalia.Utility.NativeNoise;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Fraktalia.VoxelGen.World
{
	public class WorldAlgorithm_Fractal : WorldAlgorithm
	{
		public enum FractalType
        {
			Simple,
			Mandelbulb
        }

		[Header("Fractal Settings")]
		public FractalType FractalAlgorithm;
		public Vector3 Center = new Vector3(-0.5f,-0.5f,-0.5f);

		[Range(-255,0)]
		public float Amplitude = -255;	
		public float Radius = 0.5f;
		public float FractalValue_A = 8;
		public float FractalValue_B = 48;

		[Range(1, 20)]
		public int Iterations = 5;
		
		[Range(0,0.01f)]
		public float Smoothing = 0;
		
		public int StartValue = 255;

		WorldAlgorithm_Fractal_Calculate calculate;
		
		public override void Initialize(VoxelGenerator template)
		{
			int width = template.GetBlockWidth(Depth);
			int blocks = width * width * width;

			
			calculate.Width = width;
			calculate.Blocks = blocks;
			calculate.Depth = (byte)Depth;
		}

		public override JobHandle Apply(Vector3 hash, VoxelGenerator targetGenerator, ref JobHandle handle)
		{
			int width = targetGenerator.GetBlockWidth(Depth);
			int blocks = width * width * width;
			calculate.Width = width;
			calculate.Blocks = blocks;
			calculate.Depth = (byte)Depth;

			calculate.VoxelSize = targetGenerator.GetVoxelSize(Depth);
			calculate.voxeldata = worldGenerator.modificationReservoir.GetDataArray(Depth);
			calculate.RootSize = targetGenerator.RootSize;
			calculate.StartValue = StartValue;
			calculate.Center = hash * targetGenerator.RootSize + Center * scale;
			calculate.Amplitude = Amplitude * scale;
			calculate.Radius = Radius * scale;		
			calculate.ApplyFunction = ApplyFunction;
			calculate.PostProcessFunction = PostProcessFunction;
			calculate.PermutationTable = worldGenerator.Permutation;	
			calculate.SpecialValue_A = FractalValue_A;
			calculate.SpecialValue_B = FractalValue_B;
			calculate.SpecialValue_C = Mathf.Min(20, Iterations);
			calculate.SpecialValue_E = Smoothing;
			calculate.Algorithm = FractalAlgorithm;
			return calculate.Schedule(calculate.Blocks, 64, handle);

		}
	}

	
	
	[BurstCompile]
	public struct WorldAlgorithm_Fractal_Calculate : IJobParallelFor
	{
		[ReadOnly]
		public PermutationTable_Native PermutationTable;



		[ReadOnly]
		public WorldAlgorithm_Fractal.FractalType Algorithm;
		public byte Depth;
		public float VoxelSize;
		public float RootSize;
		public int Width;
		public int Blocks;
		public Vector3 Center;
		public float Radius;
		public float SpecialValue_A;
		public float SpecialValue_B;
		public float SpecialValue_C;
		public float SpecialValue_E;
		public float Amplitude;
		public int StartValue;

		public NativeArray<NativeVoxelModificationData_Inner> voxeldata;
		internal ApplyMode ApplyFunction;
		internal PostProcess PostProcessFunction;

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


			float Worldpos_X = Center.x + Voxelpos_X;
			float Worldpos_Y = Center.y + Voxelpos_Y;
			float Worldpos_Z = Center.z + Voxelpos_Z;

			float dist = 0;


			switch (Algorithm)
            {
                case WorldAlgorithm_Fractal.FractalType.Simple:
					Ffloat3 p = new Ffloat3((Worldpos_X), (Worldpos_Y), (Worldpos_Z)) * 1 / Radius;
					dist = FractalAlgorithm_Simple(p);
					break;
                case WorldAlgorithm_Fractal.FractalType.Mandelbulb:
					p = new Ffloat3((Worldpos_X), (Worldpos_Y), (Worldpos_Z)) * 1 / Radius;
					dist = FractalAlgorithm_MandelBulb(p);
					break;
                default:
                    break;
            }

           

			NativeVoxelModificationData_Inner info = voxeldata[index];
			info.Depth = Depth;
			info.X = VoxelGenerator.ConvertLocalToInner(Voxelpos_X, RootSize);
			info.Y = VoxelGenerator.ConvertLocalToInner(Voxelpos_Y, RootSize);
			info.Z = VoxelGenerator.ConvertLocalToInner(Voxelpos_Z, RootSize);
				
			int value = StartValue + (int)(dist*Amplitude);

			value = WorldAlgorithm_PostProcesses.CalculateApplyMode(PostProcessFunction, value);
			value = WorldAlgorithm_Modes.CalculateApplyMode(ApplyFunction, info.ID, value);
			info.ID = Mathf.Clamp(value, 0, 255);

			voxeldata[index] = info;
		}

		private float sabs(float p) => Mathf.Sqrt((p) * (p) + Mathf.Max(0, SpecialValue_E));
		private float smin(float a, float b) => (a + b - sabs(a - b)) * .5f;
		private float smax(float a, float b) => (a + b + sabs(a - b)) * .5f;

		private Ffloat2x2 rotate(float a) { float c = Fmath.cos(a), s = Fmath.sin(a); return new Ffloat2x2(c, -s, s, c); }

		private float FractalAlgorithm_Simple(Ffloat3 p)
        {
			Ffloat2 mp = new Ffloat2(SpecialValue_A, SpecialValue_B);
			Ffloat3 b = new Ffloat3(.707f, .707f, 0);
			Ffloat2x2 rotateMx = rotate(mp.x);
			Ffloat2x2 rotateMy = rotate(mp.y);

			for (float i = 0; i < SpecialValue_C; i++)
			{
				p.xz = Fmath.mul(p.xz, rotateMx);
				p.xy = Fmath.mul(p.xy, rotateMy);
				p -= smin(0, Fmath.dot(p, b)) * b * 2f; b = b.zxx;
				p -= smin(0, Fmath.dot(p, b)) * b * 2f; b = b.zxz;
				p -= smin(0, Fmath.dot(p, b)) * b * 2f; b = b.xxy;
				p = p * 1.5f - .25f;
			}
			
			return (Fmath.length(p))*0.001f;
		}

		private float FractalAlgorithm_MandelBulb(Ffloat3 p)
		{
			Ffloat3 z = p;
			float dr = 1.0f;
			float r = 0.0f;
			for (int i = 0; i < SpecialValue_C; i++)
			{
				r = Fmath.length(z);
				if (r > 1.5) break;

				// convert to polar coordinates
				float theta = Fmath.acos(z.z / r);
				float phi = Fmath.atan2(z.y, z.x);

				dr = Fmath.pow(r, SpecialValue_A - 1.0f) * SpecialValue_A * dr + 1.0f;

				// scale and rotate the point
				float zr = Fmath.pow(r, SpecialValue_A);
				theta = theta * SpecialValue_A;
				phi = phi * SpecialValue_A;

				// convert back to cartesian coordinates
				z = p + zr * new Ffloat3(Fmath.sin(theta) * Fmath.cos(phi), Fmath.sin(phi) * Fmath.sin(theta), Fmath.cos(theta));
			}
			return (0.5f * Fmath.log(r) * r / dr);
		}
	}

}
