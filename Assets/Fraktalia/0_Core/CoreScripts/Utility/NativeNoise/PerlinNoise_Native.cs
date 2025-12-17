using Fraktalia.Core.Mathematics;
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Fraktalia.Utility.NativeNoise
{
	/// <summary>
	/// 262144 Voxels, No Burst
	/// 
	/// Performance 2D:		18ms
	/// Performance Fast:	11ms
	/// </summary>
	public unsafe static class PerlinNoise_Native
	{
		public static float Sample1D( float x , float Frequency, float Amplitude, ref PermutationTable_Native Perm)
		{
			x = x * Frequency;

			int ix0;
		    float fx0, fx1;
		    float s, n0, n1;
		
		    ix0 = (int)Mathf.Floor(x); 	// Integer part of x
		    fx0 = x - ix0;              // Fractional part of x
		    fx1 = fx0 - 1.0f;
			
		    s = FADE(fx0);
		
		    n0 = Grad(Perm[ix0], fx0);
		    n1 = Grad(Perm[ix0 + 1], fx1);

            return 0.25f * LERP(s, n0, n1) * Amplitude;
		}
		
		public static float Sample2D( float x, float y , float Frequency, float Amplitude, ref PermutationTable_Native Perm)
		{
			x = x * Frequency;
			y = y * Frequency;

			int ix0, iy0;
			float fx0, fy0, fx1, fy1, s, t, nx0, nx1, n0, n1;

			ix0 = (int)Mathf.Floor(x);      // Integer part of x
			iy0 = (int)Mathf.Floor(y);      // Integer part of y

			fx0 = x - ix0;              // Fractional part of x
			fy0 = y - iy0;              // Fractional part of y
			fx1 = fx0 - 1.0f;
			fy1 = fy0 - 1.0f;

			t = FADE(fy0);
			s = FADE(fx0);

			nx0 = Grad(Perm[ix0, iy0], fx0, fy0);
			nx1 = Grad(Perm[ix0, iy0 + 1], fx0, fy1);

			n0 = LERP(t, nx0, nx1);

			nx0 = Grad(Perm[ix0 + 1, iy0], fx1, fy0);
			nx1 = Grad(Perm[ix0 + 1, iy0 + 1], fx1, fy1);

			n1 = LERP(t, nx0, nx1);

			return 0.66666f * LERP(s, n0, n1) * Amplitude;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sample2DUnsafeFast(float x, float y, float Frequency, float Amplitude, void* perm, int wrap)
		{
			x = x * Frequency;
			y = y * Frequency;

			int ix0, iy0;
			float fx0, fy0, fx1, fy1, s, t, nx0, nx1, n0, n1;

			ix0 = (int)Fmath.floor(x);      // Integer part of x
			iy0 = (int)Fmath.floor(y);      // Integer part of y

			fx0 = x - ix0;              // Fractional part of x
			fy0 = y - iy0;              // Fractional part of y
			fx1 = fx0 - 1.0f;
			fy1 = fy0 - 1.0f;

			t = FADE(fy0);
			s = FADE(fx0);

			int index;

			index = (ix0 * iy0) & wrap;
			nx0 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), fx0, fy0);

			index = (ix0 * (iy0 + 1)) & wrap;
			nx1 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), fx0, fy1);

			n0 = LERP(t, nx0, nx1);

			index = ((ix0 + 1) * iy0) & wrap;
			nx0 = Grad(UnsafeUtility.ReadArrayElement<int>(perm,index), fx1, fy0);
			
			index = ((ix0 + 1) * (iy0 + 1)) & wrap;
			nx1 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index ), fx1, fy1);

			n1 = LERP(t, nx0, nx1);

			return 0.66666f * LERP(s, n0, n1) * Amplitude;
		}

		public static float Sample3D( float x, float y, float z , float Frequency, float Amplitude, ref PermutationTable_Native Perm)
		{
            x = x * Frequency;
            y = y * Frequency;
            z = z * Frequency;

            int ix0, iy0, iz0;
		    float fx0, fy0, fz0, fx1, fy1, fz1;
		    float s, t, r;
		    float nxy0, nxy1, nx0, nx1, n0, n1;
		
			ix0 = (int)Mathf.Floor(x);   		// Integer part of x
			iy0 = (int)Mathf.Floor(y);   		// Integer part of y
			iz0 = (int)Mathf.Floor(z);   		// Integer part of z
		    fx0 = x - ix0;        		        // Fractional part of x
		    fy0 = y - iy0;        		        // Fractional part of y
		    fz0 = z - iz0;        		        // Fractional part of z
		    fx1 = fx0 - 1.0f;
		    fy1 = fy0 - 1.0f;
		    fz1 = fz0 - 1.0f;
		    
		    r = FADE( fz0 );
		    t = FADE( fy0 );
		    s = FADE( fx0 );
		
			nxy0 = Grad(Perm[ix0, iy0, iz0], fx0, fy0, fz0);
		    nxy1 = Grad(Perm[ix0, iy0, iz0 + 1], fx0, fy0, fz1);
		    nx0 = LERP( r, nxy0, nxy1 );
		
		    nxy0 = Grad(Perm[ix0, iy0 + 1, iz0], fx0, fy1, fz0);
		    nxy1 = Grad(Perm[ix0, iy0 + 1, iz0 + 1], fx0, fy1, fz1);
		    nx1 = LERP( r, nxy0, nxy1 );
		
		    n0 = LERP( t, nx0, nx1 );
		
		    nxy0 = Grad(Perm[ix0 + 1, iy0, iz0], fx1, fy0, fz0);
		    nxy1 = Grad(Perm[ix0 + 1, iy0, iz0 + 1], fx1, fy0, fz1);
		    nx0 = LERP( r, nxy0, nxy1 );
		
		    nxy0 = Grad(Perm[ix0 + 1, iy0 + 1, iz0], fx1, fy1, fz0);
		   	nxy1 = Grad(Perm[ix0 + 1, iy0 + 1, iz0 + 1], fx1, fy1, fz1);
		    nx1 = LERP( r, nxy0, nxy1 );
		
		    n1 = LERP( t, nx0, nx1 );

            return 1.1111f * LERP(s, n0, n1) * Amplitude;
		}

		public static float Sample3DUnsafeFast(float x, float y, float z, float Frequency, float Amplitude, void* perm, int wrap)
		{
			x = x * Frequency;
			y = y * Frequency;
			z = z * Frequency;

			int ix0, iy0, iz0;
			float fx0, fy0, fz0, fx1, fy1, fz1;
			float s, t, r;
			float nxy0, nxy1, nx0, nx1, n0, n1;

			ix0 = (int)Mathf.Floor(x);          // Integer part of x
			iy0 = (int)Mathf.Floor(y);          // Integer part of y
			iz0 = (int)Mathf.Floor(z);          // Integer part of z
			fx0 = x - ix0;                      // Fractional part of x
			fy0 = y - iy0;                      // Fractional part of y
			fz0 = z - iz0;                      // Fractional part of z
			fx1 = fx0 - 1.0f;
			fy1 = fy0 - 1.0f;
			fz1 = fz0 - 1.0f;

			r = FADE(fz0);
			t = FADE(fy0);
			s = FADE(fx0);

			int index;
			index = (ix0 * iy0 * iz0) & wrap;
			nxy0 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), fx0, fy0, fz0);

			index = (ix0 * iy0 * (iz0 + 1)) & wrap;
			nxy1 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), fx0, fy0, fz1);
			nx0 = LERP(r, nxy0, nxy1);

			index = (ix0 * (iy0+1) * iz0) & wrap;
			nxy0 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), fx0, fy1, fz0);

			index = (ix0 * (iy0 + 1) * (iz0 + 1)) & wrap;
			nxy1 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), fx0, fy1, fz1);
			nx1 = LERP(r, nxy0, nxy1);

			n0 = LERP(t, nx0, nx1);

			index = ((ix0 + 1) * (iy0) * (iz0)) & wrap;
			nxy0 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), fx1, fy0, fz0);

			index = ((ix0 + 1) * (iy0) * (iz0+1)) & wrap;
			nxy1 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), fx1, fy0, fz1);
			nx0 = LERP(r, nxy0, nxy1);

			index = ((ix0 + 1) * (iy0+1) * (iz0)) & wrap;
			nxy0 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), fx1, fy1, fz0);

			index = ((ix0 + 1) * (iy0 + 1) * (iz0+1)) & wrap;
			nxy1 = Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), fx1, fy1, fz1);
			nx1 = LERP(r, nxy0, nxy1);

			n1 = LERP(t, nx0, nx1);

			return 1.1111f * LERP(s, n0, n1) * Amplitude;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float FADE(float t) { return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float LERP(float t, float a, float b) { return a + t * (b - a); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Grad(int hash, float x)
        {
            int h = hash & 15;
            float grad = 1.0f + (h & 7);    // Gradient value 1.0, 2.0, ..., 8.0
            if ((h & 8) != 0) grad = -grad; // Set a random sign for the gradient
            return (grad * x);              // Multiply the gradient with the distance
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Grad(int hash, float x, float y)
        {
            int h = hash & 7;           // Convert low 3 bits of hash code
            float u = h < 4 ? x : y;  // into 8 simple gradient directions,
            float v = h < 4 ? y : x;  // and compute the dot product with (x,y).
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Grad(int hash, float x, float y, float z)
        {
            int h = hash & 15;     // Convert low 4 bits of hash code into 12 simple
            float u = h < 8 ? x : y; // gradient directions, and compute dot product.
            float v = h < 4 ? y : h == 12 || h == 14 ? x : z; // Fix repeats at h = 12 to 15
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
        }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Grad(int hash, float x, float y, float z, float t)
        {
            int h = hash & 31;          // Convert low 5 bits of hash code into 32 simple
            float u = h < 24 ? x : y; // gradient directions, and compute dot product.
            float v = h < 16 ? y : z;
            float w = h < 8 ? z : t;
            return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v) + ((h & 4) != 0 ? -w : w);
        }
	}

	/// <summary>
	/// 262144 Voxels, No Burst
	/// 
	/// Performance 2D:		15ms
	/// Performance Fast:	10ms
	/// </summary>
	public unsafe static class SimplexNoise_Native
	{
		public static float Sample2D(float x, float y, float Frequency, float Amplitude, ref PermutationTable_Native Perm)
		{
			//The 0.5 is to make the scale simliar to the other noise algorithms
			x = (x) * Frequency * 0.5f;
			y = (y) * Frequency * 0.5f;

			const float F2 = 0.366025403f; // F2 = 0.5*(sqrt(3.0)-1.0)
			const float G2 = 0.211324865f; // G2 = (3.0-Math.sqrt(3.0))/6.0

			float n0, n1, n2; // Noise contributions from the three corners

			// Skew the input space to determine which simplex cell we're in
			float s = (x + y) * F2; // Hairy factor for 2D
			float xs = x + s;
			float ys = y + s;
			int i = (int)Mathf.Floor(xs);
			int j = (int)Mathf.Floor(ys);

			float t = (i + j) * G2;
			float X0 = i - t; // Unskew the cell origin back to (x,y) space
			float Y0 = j - t;
			float x0 = x - X0; // The x,y distances from the cell origin
			float y0 = y - Y0;

			// For the 2D case, the simplex shape is an equilateral triangle.
			// Determine which simplex we are in.
			int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
			if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
			else { i1 = 0; j1 = 1; }      // upper triangle, YX order: (0,0)->(0,1)->(1,1)

			// A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
			// a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
			// c = (3-sqrt(3))/6

			float x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
			float y1 = y0 - j1 + G2;
			float x2 = x0 - 1.0f + 2.0f * G2; // Offsets for last corner in (x,y) unskewed coords
			float y2 = y0 - 1.0f + 2.0f * G2;

			// Calculate the contribution from the three corners
			float t0 = 0.5f - x0 * x0 - y0 * y0;
			if (t0 < 0.0) n0 = 0.0f;
			else
			{
				t0 *= t0;
				n0 = t0 * t0 * Grad(Perm[i, j], x0, y0);
			}

			float t1 = 0.5f - x1 * x1 - y1 * y1;
			if (t1 < 0.0) n1 = 0.0f;
			else
			{
				t1 *= t1;
				n1 = t1 * t1 * Grad(Perm[i + i1, j + j1], x1, y1);
			}

			float t2 = 0.5f - x2 * x2 - y2 * y2;
			if (t2 < 0.0) n2 = 0.0f;
			else
			{
				t2 *= t2;
				n2 = t2 * t2 * Grad(Perm[i + 1, j + 1], x2, y2);
			}

			// Add contributions from each corner to get the final noise value.
			// The result is scaled to return values in the interval [-1,1].
			return 40.0f * (n0 + n1 + n2) * Amplitude;
		}

		public static float Sample3D(float x, float y, float z, float Frequency, float Amplitude, ref PermutationTable_Native Perm)
		{
			//The 0.5 is to make the scale simliar to the other noise algorithms
			x = (x) * Frequency * 0.5f;
			y = (y) * Frequency * 0.5f;
			z = (z) * Frequency * 0.5f;

			// Simple skewing factors for the 3D case
			const float F3 = 0.333333333f;
			const float G3 = 0.166666667f;

			float n0, n1, n2, n3; // Noise contributions from the four corners

			// Skew the input space to determine which simplex cell we're in
			float s = (x + y + z) * F3; // Very nice and simple skew factor for 3D
			float xs = x + s;
			float ys = y + s;
			float zs = z + s;
			int i = (int)Mathf.Floor(xs);
			int j = (int)Mathf.Floor(ys);
			int k = (int)Mathf.Floor(zs);

			float t = (i + j + k) * G3;
			float X0 = i - t; // Unskew the cell origin back to (x,y,z) space
			float Y0 = j - t;
			float Z0 = k - t;
			float x0 = x - X0; // The x,y,z distances from the cell origin
			float y0 = y - Y0;
			float z0 = z - Z0;

			// For the 3D case, the simplex shape is a slightly irregular tetrahedron.
			// Determine which simplex we are in.
			int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
			int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords

			/* This code would benefit from a backport from the GLSL version! */
			if (x0 >= y0)
			{
				if (y0 >= z0)
				{ i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z order
				else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y order
				else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y order
			}
			else
			{ // x0<y0
				if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X order
				else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X order
				else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z order
			}

			// A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
			// a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
			// a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
			// c = 1/6.

			float x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
			float y1 = y0 - j1 + G3;
			float z1 = z0 - k1 + G3;
			float x2 = x0 - i2 + 2.0f * G3; // Offsets for third corner in (x,y,z) coords
			float y2 = y0 - j2 + 2.0f * G3;
			float z2 = z0 - k2 + 2.0f * G3;
			float x3 = x0 - 1.0f + 3.0f * G3; // Offsets for last corner in (x,y,z) coords
			float y3 = y0 - 1.0f + 3.0f * G3;
			float z3 = z0 - 1.0f + 3.0f * G3;

			// Calculate the contribution from the four corners
			float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
			if (t0 < 0.0) n0 = 0.0f;
			else
			{
				t0 *= t0;
				n0 = t0 * t0 * Grad(Perm[i, j, k], x0, y0, z0);
			}

			float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
			if (t1 < 0.0) n1 = 0.0f;
			else
			{
				t1 *= t1;
				n1 = t1 * t1 * Grad(Perm[i + i1, j + j1, k + k1], x1, y1, z1);
			}

			float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
			if (t2 < 0.0) n2 = 0.0f;
			else
			{
				t2 *= t2;
				n2 = t2 * t2 * Grad(Perm[i + i2, j + j2, k + k2], x2, y2, z2);
			}

			float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
			if (t3 < 0.0) n3 = 0.0f;
			else
			{
				t3 *= t3;
				n3 = t3 * t3 * Grad(Perm[i + 1, j + 1, k + 1], x3, y3, z3);
			}

			// Add contributions from each corner to get the final noise value.
			// The result is scaled to stay just inside [-1,1]
			return 32.0f * (n0 + n1 + n2 + n3) * Amplitude;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Sample2DUnsafeFast(float x, float y, float Frequency, float Amplitude, void* perm, int wrap)
		{
			//The 0.5 is to make the scale simliar to the other noise algorithms
			x = (x) * Frequency * 0.5f;
			y = (y) * Frequency * 0.5f;

			const float F2 = 0.366025403f; // F2 = 0.5*(sqrt(3.0)-1.0)
			const float G2 = 0.211324865f; // G2 = (3.0-Math.sqrt(3.0))/6.0

			float n0, n1, n2; // Noise contributions from the three corners

			// Skew the input space to determine which simplex cell we're in
			float s = (x + y) * F2; // Hairy factor for 2D
			float xs = x + s;
			float ys = y + s;
			int i = (int)Mathf.Floor(xs);
			int j = (int)Mathf.Floor(ys);

			float t = (i + j) * G2;
			float X0 = i - t; // Unskew the cell origin back to (x,y) space
			float Y0 = j - t;
			float x0 = x - X0; // The x,y distances from the cell origin
			float y0 = y - Y0;

			// For the 2D case, the simplex shape is an equilateral triangle.
			// Determine which simplex we are in.
			int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
			if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
			else { i1 = 0; j1 = 1; }      // upper triangle, YX order: (0,0)->(0,1)->(1,1)

			// A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
			// a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
			// c = (3-sqrt(3))/6

			float x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
			float y1 = y0 - j1 + G2;
			float x2 = x0 - 1.0f + 2.0f * G2; // Offsets for last corner in (x,y) unskewed coords
			float y2 = y0 - 1.0f + 2.0f * G2;
			int index;
			// Calculate the contribution from the three corners
			float t0 = 0.5f - x0 * x0 - y0 * y0;
			if (t0 < 0.0) n0 = 0.0f;
			else
			{
				t0 *= t0;

				index = (i * j) & wrap;
				n0 = t0 * t0 * Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), x0, y0);
			}

			float t1 = 0.5f - x1 * x1 - y1 * y1;
			if (t1 < 0.0) n1 = 0.0f;
			else
			{
				t1 *= t1;

				index = (i + i1 * j + j1) & wrap;
				n1 = t1 * t1 * Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), x1, y1);
			}

			float t2 = 0.5f - x2 * x2 - y2 * y2;
			if (t2 < 0.0) n2 = 0.0f;
			else
			{
				t2 *= t2;

				index = (i + 1 * j + 1) & wrap;
				n2 = t2 * t2 * Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), x2, y2);
			}

			// Add contributions from each corner to get the final noise value.
			// The result is scaled to return values in the interval [-1,1].
			return 40.0f * (n0 + n1 + n2) * Amplitude;
		}

		public static float Sample3DUnsafeFast(float x, float y, float z, float Frequency, float Amplitude, void* perm, int wrap)
		{
			//The 0.5 is to make the scale simliar to the other noise algorithms
			x = (x) * Frequency * 0.5f;
			y = (y) * Frequency * 0.5f;
			z = (z) * Frequency * 0.5f;

			// Simple skewing factors for the 3D case
			const float F3 = 0.333333333f;
			const float G3 = 0.166666667f;

			float n0, n1, n2, n3; // Noise contributions from the four corners

			// Skew the input space to determine which simplex cell we're in
			float s = (x + y + z) * F3; // Very nice and simple skew factor for 3D
			float xs = x + s;
			float ys = y + s;
			float zs = z + s;
			int i = (int)Mathf.Floor(xs);
			int j = (int)Mathf.Floor(ys);
			int k = (int)Mathf.Floor(zs);

			float t = (i + j + k) * G3;
			float X0 = i - t; // Unskew the cell origin back to (x,y,z) space
			float Y0 = j - t;
			float Z0 = k - t;
			float x0 = x - X0; // The x,y,z distances from the cell origin
			float y0 = y - Y0;
			float z0 = z - Z0;

			// For the 3D case, the simplex shape is a slightly irregular tetrahedron.
			// Determine which simplex we are in.
			int i1, j1, k1; // Offsets for second corner of simplex in (i,j,k) coords
			int i2, j2, k2; // Offsets for third corner of simplex in (i,j,k) coords

			/* This code would benefit from a backport from the GLSL version! */
			if (x0 >= y0)
			{
				if (y0 >= z0)
				{ i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // X Y Z order
				else if (x0 >= z0) { i1 = 1; j1 = 0; k1 = 0; i2 = 1; j2 = 0; k2 = 1; } // X Z Y order
				else { i1 = 0; j1 = 0; k1 = 1; i2 = 1; j2 = 0; k2 = 1; } // Z X Y order
			}
			else
			{ // x0<y0
				if (y0 < z0) { i1 = 0; j1 = 0; k1 = 1; i2 = 0; j2 = 1; k2 = 1; } // Z Y X order
				else if (x0 < z0) { i1 = 0; j1 = 1; k1 = 0; i2 = 0; j2 = 1; k2 = 1; } // Y Z X order
				else { i1 = 0; j1 = 1; k1 = 0; i2 = 1; j2 = 1; k2 = 0; } // Y X Z order
			}

			// A step of (1,0,0) in (i,j,k) means a step of (1-c,-c,-c) in (x,y,z),
			// a step of (0,1,0) in (i,j,k) means a step of (-c,1-c,-c) in (x,y,z), and
			// a step of (0,0,1) in (i,j,k) means a step of (-c,-c,1-c) in (x,y,z), where
			// c = 1/6.

			float x1 = x0 - i1 + G3; // Offsets for second corner in (x,y,z) coords
			float y1 = y0 - j1 + G3;
			float z1 = z0 - k1 + G3;
			float x2 = x0 - i2 + 2.0f * G3; // Offsets for third corner in (x,y,z) coords
			float y2 = y0 - j2 + 2.0f * G3;
			float z2 = z0 - k2 + 2.0f * G3;
			float x3 = x0 - 1.0f + 3.0f * G3; // Offsets for last corner in (x,y,z) coords
			float y3 = y0 - 1.0f + 3.0f * G3;
			float z3 = z0 - 1.0f + 3.0f * G3;
			int index;
			// Calculate the contribution from the four corners
			float t0 = 0.6f - x0 * x0 - y0 * y0 - z0 * z0;
			if (t0 < 0.0) n0 = 0.0f;
			else
			{
				t0 *= t0;

				index = (i * j * k) & wrap;

				n0 = t0 * t0 * Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), x0, y0, z0);
			}

			float t1 = 0.6f - x1 * x1 - y1 * y1 - z1 * z1;
			if (t1 < 0.0) n1 = 0.0f;
			else
			{
				t1 *= t1;

				index = ((i + i1) * j * (k + k1)) & wrap;

				n1 = t1 * t1 * Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), x1, y1, z1);
			}

			float t2 = 0.6f - x2 * x2 - y2 * y2 - z2 * z2;
			if (t2 < 0.0) n2 = 0.0f;
			else
			{
				t2 *= t2;

				index = ((i + i2) * (j + j2) * (k + k2)) & wrap;

				n2 = t2 * t2 * Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), x2, y2, z2);
			}

			float t3 = 0.6f - x3 * x3 - y3 * y3 - z3 * z3;
			if (t3 < 0.0) n3 = 0.0f;
			else
			{
				t3 *= t3;

				index = ((i + 1) * (j + 1) * (k + 1)) & wrap;

				n3 = t3 * t3 * Grad(UnsafeUtility.ReadArrayElement<int>(perm, index), x3, y3, z3);
			}

			// Add contributions from each corner to get the final noise value.
			// The result is scaled to stay just inside [-1,1]
			return 32.0f * (n0 + n1 + n2 + n3) * Amplitude;
		}



		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float FADE(float t) { return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float LERP(float t, float a, float b) { return a + t * (b - a); }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Grad(int hash, float x)
		{
			int h = hash & 15;
			float grad = 1.0f + (h & 7);    // Gradient value 1.0, 2.0, ..., 8.0
			if ((h & 8) != 0) grad = -grad; // Set a random sign for the gradient
			return (grad * x);              // Multiply the gradient with the distance
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Grad(int hash, float x, float y)
		{
			int h = hash & 7;           // Convert low 3 bits of hash code
			float u = h < 4 ? x : y;  // into 8 simple gradient directions,
			float v = h < 4 ? y : x;  // and compute the dot product with (x,y).
			return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -2.0f * v : 2.0f * v);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Grad(int hash, float x, float y, float z)
		{
			int h = hash & 15;     // Convert low 4 bits of hash code into 12 simple
			float u = h < 8 ? x : y; // gradient directions, and compute dot product.
			float v = h < 4 ? y : h == 12 || h == 14 ? x : z; // Fix repeats at h = 12 to 15
			return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Grad(int hash, float x, float y, float z, float t)
		{
			int h = hash & 31;          // Convert low 5 bits of hash code into 32 simple
			float u = h < 24 ? x : y; // gradient directions, and compute dot product.
			float v = h < 16 ? y : z;
			float w = h < 8 ? z : t;
			return ((h & 1) != 0 ? -u : u) + ((h & 2) != 0 ? -v : v) + ((h & 4) != 0 ? -w : w);
		}
	}

	/// <summary>
	/// 262144 Voxels, No Burst
	/// 
	/// Performance 2D:		176ms
	/// Performance Fast:	80ms
	/// </summary>
	public unsafe static class VoronoiNoise_Native
	{
		public enum VORONOI_DISTANCE { EUCLIDIAN, MANHATTAN, CHEBYSHEV };

		public enum VORONOI_COMBINATION { D0, D1_D0, D2_D0 };

		public static float Sample2D(float x, float y, float Frequency, float Amplitude, ref PermutationTable_Native Perm, VORONOI_DISTANCE distance, VORONOI_COMBINATION combination )
		{
			//The 0.75 is to make the scale simliar to the other noise algorithms
			x = (x) * Frequency * 0.75f;
			y = (y) * Frequency * 0.75f;

			int lastRandom, numberFeaturePoints;
			float randomDiffX, randomDiffY;
			float featurePointX, featurePointY;
			int cubeX, cubeY;

			Ffloat3 distanceArray = new Ffloat3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

			//1. Determine which cube the evaluation point is in
			int evalCubeX = (int)Mathf.Floor(x);
			int evalCubeY = (int)Mathf.Floor(y);

			for (int i = -1; i < 2; ++i)
			{
				for (int j = -1; j < 2; ++j)
				{
					cubeX = evalCubeX + i;
					cubeY = evalCubeY + j;

					//2. Generate a reproducible random number generator for the cube
					lastRandom = Perm[cubeX, cubeY];

					//3. Determine how many feature points are in the cube
					numberFeaturePoints = ProbLookup(lastRandom * Perm.Inverse);

					//4. Randomly place the feature points in the cube
					for (int l = 0; l < numberFeaturePoints; ++l)
					{
						lastRandom = Perm[lastRandom];
						randomDiffX = lastRandom * Perm.Inverse;

						lastRandom = Perm[lastRandom];
						randomDiffY = lastRandom * Perm.Inverse;

						featurePointX = randomDiffX + cubeX;
						featurePointY = randomDiffY + cubeY;

						//5. Find the feature point closest to the evaluation point. 
						//This is done by inserting the distances to the feature points into a sorted list
						distanceArray = Insert(distanceArray, Distance2(x, y, featurePointX, featurePointY, distance));
					}

					//6. Check the neighboring cubes to ensure their are no closer evaluation points.
					// This is done by repeating steps 1 through 5 above for each neighboring cube
				}
			}

			return Combine(distanceArray, combination) * Amplitude;
		}

		public static float Sample2DUnsafeFast(float x, float y, float Frequency, float Amplitude, void* perm, int wrap, float inverse, VORONOI_DISTANCE distance, VORONOI_COMBINATION combination)
		{
			//The 0.75 is to make the scale simliar to the other noise algorithms
			x = (x) * Frequency * 0.75f;
			y = (y) * Frequency * 0.75f;

			int lastRandom, numberFeaturePoints;
			float randomDiffX, randomDiffY;
			float featurePointX, featurePointY;
			int cubeX, cubeY;
			int index;

			Ffloat3 distanceArray = new Ffloat3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

			//1. Determine which cube the evaluation point is in
			int evalCubeX = (int)Mathf.Floor(x);
			int evalCubeY = (int)Mathf.Floor(y);

			for (int i = -1; i < 2; ++i)
			{
				for (int j = -1; j < 2; ++j)
				{
					cubeX = evalCubeX + i;
					cubeY = evalCubeY + j;

					//2. Generate a reproducible random number generator for the cube
					index = (cubeX + cubeY) & wrap;
					lastRandom = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;
					//index = (lastRandom + cubeY) & wrap;
					//lastRandom = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;

					//3. Determine how many feature points are in the cube
					numberFeaturePoints = ProbLookup(lastRandom * inverse);

					//4. Randomly place the feature points in the cube
					for (int l = 0; l < numberFeaturePoints; ++l)
					{
						index = lastRandom & wrap;
						lastRandom = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;
						randomDiffX = lastRandom * inverse;

						index = lastRandom & wrap;
						lastRandom = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;
						randomDiffY = lastRandom * inverse;

						featurePointX = randomDiffX + cubeX;
						featurePointY = randomDiffY + cubeY;

						//5. Find the feature point closest to the evaluation point. 
						//This is done by inserting the distances to the feature points into a sorted list
						distanceArray = Insert(distanceArray, Distance2(x, y, featurePointX, featurePointY, distance));
					}

					//6. Check the neighboring cubes to ensure their are no closer evaluation points.
					// This is done by repeating steps 1 through 5 above for each neighboring cube
				}
			}

			return Combine(distanceArray, combination) * Amplitude;
		}

		public static float Sample3D(float x, float y, float z, float Frequency, float Amplitude, ref PermutationTable_Native Perm, VORONOI_DISTANCE distance, VORONOI_COMBINATION combination)
		{
			//The 0.75 is to make the scale simliar to the other noise algorithms
			x = (x) * Frequency * 0.75f;
			y = (y) * Frequency * 0.75f;
			z = (z) * Frequency * 0.75f;

			int lastRandom, numberFeaturePoints;
			float randomDiffX, randomDiffY, randomDiffZ;
			float featurePointX, featurePointY, featurePointZ;
			int cubeX, cubeY, cubeZ;

			Ffloat3 distanceArray = new Ffloat3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

			//1. Determine which cube the evaluation point is in
			int evalCubeX = (int)Mathf.Floor(x);
			int evalCubeY = (int)Mathf.Floor(y);
			int evalCubeZ = (int)Mathf.Floor(z);

			for (int i = -1; i < 2; ++i)
			{
				for (int j = -1; j < 2; ++j)
				{
					for (int k = -1; k < 2; ++k)
					{
						cubeX = evalCubeX + i;
						cubeY = evalCubeY + j;
						cubeZ = evalCubeZ + k;

						//2. Generate a reproducible random number generator for the cube
						lastRandom = Perm[cubeX, cubeY, cubeZ];

						//3. Determine how many feature points are in the cube
						numberFeaturePoints = ProbLookup(lastRandom * Perm.Inverse);

						//4. Randomly place the feature points in the cube
						for (int l = 0; l < numberFeaturePoints; ++l)
						{
							lastRandom = Perm[lastRandom];
							randomDiffX = lastRandom * Perm.Inverse;

							lastRandom = Perm[lastRandom];
							randomDiffY = lastRandom * Perm.Inverse;

							lastRandom = Perm[lastRandom];
							randomDiffZ = lastRandom * Perm.Inverse;

							featurePointX = randomDiffX + cubeX;
							featurePointY = randomDiffY + cubeY;
							featurePointZ = randomDiffZ + cubeZ;

							//5. Find the feature point closest to the evaluation point. 
							//This is done by inserting the distances to the feature points into a sorted list
							distanceArray = Insert(distanceArray, Distance3(x, y, z, featurePointX, featurePointY, featurePointZ, distance));
						}

						//6. Check the neighboring cubes to ensure their are no closer evaluation points.
						// This is done by repeating steps 1 through 5 above for each neighboring cube
					}
				}
			}

			return Combine(distanceArray, combination) * Amplitude;
		}

		public static float Sample3DUnsafeFast(float x, float y, float z, float Frequency, float Amplitude, void* perm, int wrap, float inverse, VORONOI_DISTANCE distance, VORONOI_COMBINATION combination)
		{
			//The 0.75 is to make the scale simliar to the other noise algorithms
			x = (x) * Frequency * 0.75f;
			y = (y) * Frequency * 0.75f;
			z = (z) * Frequency * 0.75f;

			int lastRandom, numberFeaturePoints;
			float randomDiffX, randomDiffY, randomDiffZ;
			float featurePointX, featurePointY, featurePointZ;
			int cubeX, cubeY, cubeZ;
			int index;
			Ffloat3 distanceArray = new Ffloat3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

			//1. Determine which cube the evaluation point is in
			int evalCubeX = (int)Mathf.Floor(x);
			int evalCubeY = (int)Mathf.Floor(y);
			int evalCubeZ = (int)Mathf.Floor(z);

			for (int i = -1; i < 2; ++i)
			{
				for (int j = -1; j < 2; ++j)
				{
					for (int k = -1; k < 2; ++k)
					{
						cubeX = evalCubeX + i;
						cubeY = evalCubeY + j;
						cubeZ = evalCubeZ + k;

						//2. Generate a reproducible random number generator for the cube
						index = (cubeX + cubeY + cubeZ) & wrap;
						lastRandom = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;

						//3. Determine how many feature points are in the cube
						numberFeaturePoints = ProbLookup(lastRandom * inverse);

						//4. Randomly place the feature points in the cube
						for (int l = 0; l < numberFeaturePoints; ++l)
						{
							index = lastRandom & wrap;
							lastRandom = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;
							randomDiffX = lastRandom * inverse;

							index = lastRandom & wrap;
							lastRandom = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;
							randomDiffY = lastRandom * inverse;

							index = lastRandom & wrap;
							lastRandom = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;
							randomDiffZ = lastRandom * inverse;

							featurePointX = randomDiffX + cubeX;
							featurePointY = randomDiffY + cubeY;
							featurePointZ = randomDiffZ + cubeZ;

							//5. Find the feature point closest to the evaluation point. 
							//This is done by inserting the distances to the feature points into a sorted list
							distanceArray = Insert(distanceArray, Distance3(x, y, z, featurePointX, featurePointY, featurePointZ, distance));
						}

						//6. Check the neighboring cubes to ensure their are no closer evaluation points.
						// This is done by repeating steps 1 through 5 above for each neighboring cube
					}
				}
			}

			return Combine(distanceArray, combination) * Amplitude;
		}


		private static float Distance1(float p1x, float p2x, VORONOI_DISTANCE distance)
		{
			switch (distance)
			{
				case VORONOI_DISTANCE.EUCLIDIAN:
					return (p1x - p2x) * (p1x - p2x);

				case VORONOI_DISTANCE.MANHATTAN:
					return Math.Abs(p1x - p2x);

				case VORONOI_DISTANCE.CHEBYSHEV:
					return Math.Abs(p1x - p2x);
			}

			return 0;
		}

		private static float Distance2(float p1x, float p1y, float p2x, float p2y, VORONOI_DISTANCE distance)
		{
			switch (distance)
			{
				case VORONOI_DISTANCE.EUCLIDIAN:
					return (p1x - p2x) * (p1x - p2x) + (p1y - p2y) * (p1y - p2y);

				case VORONOI_DISTANCE.MANHATTAN:
					return Math.Abs(p1x - p2x) + Math.Abs(p1y - p2y);

				case VORONOI_DISTANCE.CHEBYSHEV:
					return Math.Max(Math.Abs(p1x - p2x), Math.Abs(p1y - p2y));
			}

			return 0;
		}

		private static float Distance3(float p1x, float p1y, float p1z, float p2x, float p2y, float p2z, VORONOI_DISTANCE distance)
		{
			switch (distance)
			{
				case VORONOI_DISTANCE.EUCLIDIAN:
					return (p1x - p2x) * (p1x - p2x) + (p1y - p2y) * (p1y - p2y) + (p1z - p2z) * (p1z - p2z);

				case VORONOI_DISTANCE.MANHATTAN:
					return Math.Abs(p1x - p2x) + Math.Abs(p1y - p2y) + Math.Abs(p1z - p2z);

				case VORONOI_DISTANCE.CHEBYSHEV:
					return Math.Max(Math.Max(Math.Abs(p1x - p2x), Math.Abs(p1y - p2y)), Math.Abs(p1z - p2z));
			}

			return 0;
		}

		private static float Combine(Ffloat3 arr, VORONOI_COMBINATION combination)
		{
			switch (combination)
			{
				case VORONOI_COMBINATION.D0:
					return arr.x;

				case VORONOI_COMBINATION.D1_D0:
					return arr.y - arr.x;

				case VORONOI_COMBINATION.D2_D0:
					return arr.z - arr.x;
			}

			return 0;
		}

		/// <summary>
		/// Given a uniformly distributed random number this function returns the number of feature points in a given cube.
		/// </summary>
		/// <param name="value">a uniformly distributed random number</param>
		/// <returns>The number of feature points in a cube.</returns>
		static int ProbLookup(float value)
		{
			//Poisson Distribution
			if (value < 0.0915781944272058) return 1;
			if (value < 0.238103305510735) return 2;
			if (value < 0.433470120288774) return 3;
			if (value < 0.628836935299644) return 4;
			if (value < 0.785130387122075) return 5;
			if (value < 0.889326021747972) return 6;
			if (value < 0.948866384324819) return 7;
			if (value < 0.978636565613243) return 8;

			return 9;
		}

		/// <summary>
		/// Inserts value into array using insertion sort. If the value is greater than the largest value in the array
		/// it will not be added to the array.
		/// </summary>
		/// <param name="arr">The array to insert the value into.</param>
		/// <param name="value">The value to insert into the array.</param>
		static Ffloat3 Insert(Ffloat3 arr, float value)
		{
			float temp;

			if (value <= arr.z)
			{
				temp = arr.z;
				arr.z = value;
				value = temp;
			}

			if (value <= arr.y)
			{
				temp = arr.y;
				arr.y = value;
				value = temp;
			}

			if (value <= arr.x)
			{
				arr.x = value;
			}

			return arr;
		}



	}

	/// <summary>
	/// 262144 Voxels, No Burst
	/// 
	/// Performance 2D:		62ms
	/// Performance Fast:	10ms
	/// </summary>
	public unsafe static class WorleyNoise_Native
	{
		private const float K = 1.0f / 7.0f;
		private const float Ko = 3.0f / 7.0f;
		private static readonly Ffloat3 OFFSET_F = new Ffloat3(-0.5f, 0.5f, 1.5f);

		public static float Sample2D(float x, float y, float Frequency, float Amplitude, float Jitter, ref PermutationTable_Native Perm, VoronoiNoise_Native.VORONOI_DISTANCE distance, VoronoiNoise_Native.VORONOI_COMBINATION combination)
		{
			x = (x) * Frequency;
			y = (y) * Frequency;

			int Pi0 = (int)Mathf.Floor(x);
			int Pi1 = (int)Mathf.Floor(y);

			float Pf0 = Frac(x);
			float Pf1 = Frac(y);

			Ffloat3 pX = new Ffloat3(Perm[Pi0 - 1], Perm[Pi0], Perm[Pi0 + 1]);
			
			float d0, d1, d2;
			float F0 = float.PositiveInfinity;
			float F1 = float.PositiveInfinity;
			float F2 = float.PositiveInfinity;

			int px, py, pz;
			float oxx, oxy, oxz;
			float oyx, oyy, oyz;

			for (int i = 0; i < 3; i++)
			{
				px = Perm[(int)pX[i], Pi1 - 1];
				py = Perm[(int)pX[i], Pi1];
				pz = Perm[(int)pX[i], Pi1 + 1];

				oxx = Frac(px * K) - Ko;
				oxy = Frac(py * K) - Ko;
				oxz = Frac(pz * K) - Ko;

				oyx = Mod(Mathf.Floor(px * K), 7.0f) * K - Ko;
				oyy = Mod(Mathf.Floor(py * K), 7.0f) * K - Ko;
				oyz = Mod(Mathf.Floor(pz * K), 7.0f) * K - Ko;

				d0 = Distance2(Pf0, Pf1, OFFSET_F[i] + Jitter * oxx, -0.5f + Jitter * oyx, distance);
				d1 = Distance2(Pf0, Pf1, OFFSET_F[i] + Jitter * oxy, 0.5f + Jitter * oyy, distance);
				d2 = Distance2(Pf0, Pf1, OFFSET_F[i] + Jitter * oxz, 1.5f + Jitter * oyz, distance);

				if (d0 < F0) { F2 = F1; F1 = F0; F0 = d0; }
				else if (d0 < F1) { F2 = F1; F1 = d0; }
				else if (d0 < F2) { F2 = d0; }

				if (d1 < F0) { F2 = F1; F1 = F0; F0 = d1; }
				else if (d1 < F1) { F2 = F1; F1 = d1; }
				else if (d1 < F2) { F2 = d1; }

				if (d2 < F0) { F2 = F1; F1 = F0; F0 = d2; }
				else if (d2 < F1) { F2 = F1; F1 = d2; }
				else if (d2 < F2) { F2 = d2; }

			}

			return Combine(F0, F1, F2, combination) * Amplitude;
		}

		public static float Sample2DUnsafeFast(float x, float y, float Frequency, float Amplitude, float Jitter, void* perm, int wrap, VoronoiNoise_Native.VORONOI_DISTANCE distance, VoronoiNoise_Native.VORONOI_COMBINATION combination)
		{
			x = (x) * Frequency;
			y = (y) * Frequency;

			int Pi0 = (int)Mathf.Floor(x);
			int Pi1 = (int)Mathf.Floor(y);

			float Pf0 = Frac(x);
			float Pf1 = Frac(y);

			Ffloat3 pX = new Ffloat3(
				UnsafeUtility.ReadArrayElement<int>(perm, (Pi0 - 1) & wrap) & 255,
				UnsafeUtility.ReadArrayElement<int>(perm, Pi0 & wrap) & 255,
				UnsafeUtility.ReadArrayElement<int>(perm, (Pi0 + 1) & wrap) & 255);

			float d0, d1, d2;
			float F0 = float.PositiveInfinity;
			float F1 = float.PositiveInfinity;
			float F2 = float.PositiveInfinity;

			int px, py, pz;
			float oxx, oxy, oxz;
			float oyx, oyy, oyz;

			int pXValue;
			for (int i = 0; i < 3; i++)
			{
				pXValue = (int)pX[i];
				px = UnsafeUtility.ReadArrayElement<int>(perm, (pXValue * (Pi1 - 1)) & wrap) & 255;
				py = UnsafeUtility.ReadArrayElement<int>(perm, (pXValue * Pi1) & wrap) & 255;
				pz = UnsafeUtility.ReadArrayElement<int>(perm, (pXValue * (Pi1 + 1)) & wrap) & 255;

				oxx = Frac(px * K) - Ko;
				oxy = Frac(py * K) - Ko;
				oxz = Frac(pz * K) - Ko;

				oyx = Mod(Mathf.Floor(px * K), 7.0f) * K - Ko;
				oyy = Mod(Mathf.Floor(py * K), 7.0f) * K - Ko;
				oyz = Mod(Mathf.Floor(pz * K), 7.0f) * K - Ko;

				d0 = Distance2(Pf0, Pf1, OFFSET_F[i] + Jitter * oxx, -0.5f + Jitter * oyx, distance);
				d1 = Distance2(Pf0, Pf1, OFFSET_F[i] + Jitter * oxy, 0.5f + Jitter * oyy, distance);
				d2 = Distance2(Pf0, Pf1, OFFSET_F[i] + Jitter * oxz, 1.5f + Jitter * oyz, distance);

				if (d0 < F0) { F2 = F1; F1 = F0; F0 = d0; }
				else if (d0 < F1) { F2 = F1; F1 = d0; }
				else if (d0 < F2) { F2 = d0; }

				if (d1 < F0) { F2 = F1; F1 = F0; F0 = d1; }
				else if (d1 < F1) { F2 = F1; F1 = d1; }
				else if (d1 < F2) { F2 = d1; }

				if (d2 < F0) { F2 = F1; F1 = F0; F0 = d2; }
				else if (d2 < F1) { F2 = F1; F1 = d2; }
				else if (d2 < F2) { F2 = d2; }

			}

			return Combine(F0, F1, F2, combination) * Amplitude;
		}

		public static float Sample3D(float x, float y, float z, float Frequency, float Amplitude, float Jitter, ref PermutationTable_Native Perm, VoronoiNoise_Native.VORONOI_DISTANCE distance, VoronoiNoise_Native.VORONOI_COMBINATION combination)
		{

			x = (x) * Frequency;
			y = (y) * Frequency;
			z = (z) * Frequency;

			int Pi0 = (int)Mathf.Floor(x);
			int Pi1 = (int)Mathf.Floor(y);
			int Pi2 = (int)Mathf.Floor(z);

			float Pf0 = Frac(x);
			float Pf1 = Frac(y);
			float Pf2 = Frac(z);

			Ffloat3 pX = new Ffloat3(Perm[Pi0 - 1], Perm[Pi0], Perm[Pi0 + 1]);
		
			Ffloat3 pY = new Ffloat3(Perm[Pi1 - 1], Perm[Pi1], Perm[Pi1 + 1]);
			
			float d0, d1, d2;
			float F0 = 1e6f;
			float F1 = 1e6f;
			float F2 = 1e6f;

			int px, py, pz;
			float oxx, oxy, oxz;
			float oyx, oyy, oyz;
			float ozx, ozy, ozz;

			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{

					px = Perm[(int)pX[i], (int)pY[j], Pi2 - 1];
					py = Perm[(int)pX[i], (int)pY[j], Pi2];
					pz = Perm[(int)pX[i], (int)pY[j], Pi2 + 1];

					oxx = Frac(px * K) - Ko;
					oxy = Frac(py * K) - Ko;
					oxz = Frac(pz * K) - Ko;

					oyx = Mod(Mathf.Floor(px * K), 7.0f) * K - Ko;
					oyy = Mod(Mathf.Floor(py * K), 7.0f) * K - Ko;
					oyz = Mod(Mathf.Floor(pz * K), 7.0f) * K - Ko;

					px = Perm[px];
					py = Perm[py];
					pz = Perm[pz];

					ozx = Frac(px * K) - Ko;
					ozy = Frac(py * K) - Ko;
					ozz = Frac(pz * K) - Ko;

					d0 = Distance3(Pf0, Pf1, Pf2, OFFSET_F[i] + Jitter * oxx, OFFSET_F[j] + Jitter * oyx, -0.5f + Jitter * ozx, distance);
					d1 = Distance3(Pf0, Pf1, Pf2, OFFSET_F[i] + Jitter * oxy, OFFSET_F[j] + Jitter * oyy, 0.5f + Jitter * ozy, distance);
					d2 = Distance3(Pf0, Pf1, Pf2, OFFSET_F[i] + Jitter * oxz, OFFSET_F[j] + Jitter * oyz, 1.5f + Jitter * ozz, distance);

					if (d0 < F0) { F2 = F1; F1 = F0; F0 = d0; }
					else if (d0 < F1) { F2 = F1; F1 = d0; }
					else if (d0 < F2) { F2 = d0; }

					if (d1 < F0) { F2 = F1; F1 = F0; F0 = d1; }
					else if (d1 < F1) { F2 = F1; F1 = d1; }
					else if (d1 < F2) { F2 = d1; }

					if (d2 < F0) { F2 = F1; F1 = F0; F0 = d2; }
					else if (d2 < F1) { F2 = F1; F1 = d2; }
					else if (d2 < F2) { F2 = d2; }
				}
			}

			return Combine(F0, F1, F2, combination) * Amplitude;
		}

		public static float Sample3DUnsafeFast(float x, float y, float z, float Frequency, float Amplitude, float Jitter, void* perm, int wrap, VoronoiNoise_Native.VORONOI_DISTANCE distance, VoronoiNoise_Native.VORONOI_COMBINATION combination)
		{

			x = (x) * Frequency;
			y = (y) * Frequency;
			z = (z) * Frequency;

			int Pi0 = (int)Mathf.Floor(x);
			int Pi1 = (int)Mathf.Floor(y);
			int Pi2 = (int)Mathf.Floor(z);

			float Pf0 = Frac(x);
			float Pf1 = Frac(y);
			float Pf2 = Frac(z);

			Ffloat3 pX = new Ffloat3(
				UnsafeUtility.ReadArrayElement<int>(perm, (Pi0 - 1) & wrap) & 255,
				UnsafeUtility.ReadArrayElement<int>(perm, Pi0 & wrap) & 255,
				UnsafeUtility.ReadArrayElement<int>(perm, (Pi0 + 1) & wrap) & 255);

			Ffloat3 pY = new Ffloat3(
				UnsafeUtility.ReadArrayElement<int>(perm, (Pi1 - 1) & wrap) & 255,
				UnsafeUtility.ReadArrayElement<int>(perm, Pi1 & wrap) & 255,
				UnsafeUtility.ReadArrayElement<int>(perm, (Pi1 + 1) & wrap) & 255);

			float d0, d1, d2;
			float F0 = 1e6f;
			float F1 = 1e6f;
			float F2 = 1e6f;

			int px, py, pz;
			float oxx, oxy, oxz;
			float oyx, oyy, oyz;
			float ozx, ozy, ozz;
			int index;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					int pXValue = (int)pX[i];
					int pYValue = (int)pY[j];

					index = (pXValue * pYValue * (Pi2 - 1)) & wrap;
					px = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;

					index = (pXValue * pYValue * (Pi2)) & wrap;
					py = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;

					index = (pXValue * pYValue * (Pi2 + 1)) & wrap;
					pz = UnsafeUtility.ReadArrayElement<int>(perm, index) & 255;

					oxx = Frac(px * K) - Ko;
					oxy = Frac(py * K) - Ko;
					oxz = Frac(pz * K) - Ko;

					oyx = Mod(Mathf.Floor(px * K), 7.0f) * K - Ko;
					oyy = Mod(Mathf.Floor(py * K), 7.0f) * K - Ko;
					oyz = Mod(Mathf.Floor(pz * K), 7.0f) * K - Ko;

					px = UnsafeUtility.ReadArrayElement<int>(perm, px & wrap);
					py = UnsafeUtility.ReadArrayElement<int>(perm, py & wrap);
					pz = UnsafeUtility.ReadArrayElement<int>(perm, pz & wrap);

					ozx = Frac(px * K) - Ko;
					ozy = Frac(py * K) - Ko;
					ozz = Frac(pz * K) - Ko;

					d0 = Distance3(Pf0, Pf1, Pf2, OFFSET_F[i] + Jitter * oxx, OFFSET_F[j] + Jitter * oyx, -0.5f + Jitter * ozx, distance);
					d1 = Distance3(Pf0, Pf1, Pf2, OFFSET_F[i] + Jitter * oxy, OFFSET_F[j] + Jitter * oyy, 0.5f + Jitter * ozy, distance);
					d2 = Distance3(Pf0, Pf1, Pf2, OFFSET_F[i] + Jitter * oxz, OFFSET_F[j] + Jitter * oyz, 1.5f + Jitter * ozz, distance);

					if (d0 < F0) { F2 = F1; F1 = F0; F0 = d0; }
					else if (d0 < F1) { F2 = F1; F1 = d0; }
					else if (d0 < F2) { F2 = d0; }

					if (d1 < F0) { F2 = F1; F1 = F0; F0 = d1; }
					else if (d1 < F1) { F2 = F1; F1 = d1; }
					else if (d1 < F2) { F2 = d1; }

					if (d2 < F0) { F2 = F1; F1 = F0; F0 = d2; }
					else if (d2 < F1) { F2 = F1; F1 = d2; }
					else if (d2 < F2) { F2 = d2; }
				}
			}

			return Combine(F0, F1, F2, combination) * Amplitude;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Distance1(float p1x, float p2x, VoronoiNoise_Native.VORONOI_DISTANCE distance)
		{
			switch (distance)
			{
				case VoronoiNoise_Native.VORONOI_DISTANCE.EUCLIDIAN:
					return (p1x - p2x) * (p1x - p2x);

				case VoronoiNoise_Native.VORONOI_DISTANCE.MANHATTAN:
					return Math.Abs(p1x - p2x);

				case VoronoiNoise_Native.VORONOI_DISTANCE.CHEBYSHEV:
					return Math.Abs(p1x - p2x);
			}

			return 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Distance2(float p1x, float p1y, float p2x, float p2y, VoronoiNoise_Native.VORONOI_DISTANCE distance)
		{
			switch (distance)
			{
				case VoronoiNoise_Native.VORONOI_DISTANCE.EUCLIDIAN:
					return (p1x - p2x) * (p1x - p2x) + (p1y - p2y) * (p1y - p2y);

				case VoronoiNoise_Native.VORONOI_DISTANCE.MANHATTAN:
					return Math.Abs(p1x - p2x) + Math.Abs(p1y - p2y);

				case VoronoiNoise_Native.VORONOI_DISTANCE.CHEBYSHEV:
					return Math.Max(Math.Abs(p1x - p2x), Math.Abs(p1y - p2y));
			}

			return 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Distance3(float p1x, float p1y, float p1z, float p2x, float p2y, float p2z, VoronoiNoise_Native.VORONOI_DISTANCE distance)
		{
			switch (distance)
			{
				case VoronoiNoise_Native.VORONOI_DISTANCE.EUCLIDIAN:
					return (p1x - p2x) * (p1x - p2x) + (p1y - p2y) * (p1y - p2y) + (p1z - p2z) * (p1z - p2z);

				case VoronoiNoise_Native.VORONOI_DISTANCE.MANHATTAN:
					return Math.Abs(p1x - p2x) + Math.Abs(p1y - p2y) + Math.Abs(p1z - p2z);

				case VoronoiNoise_Native.VORONOI_DISTANCE.CHEBYSHEV:
					return Math.Max(Math.Max(Math.Abs(p1x - p2x), Math.Abs(p1y - p2y)), Math.Abs(p1z - p2z));
			}

			return 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Combine(float f0, float f1, float f2, VoronoiNoise_Native.VORONOI_COMBINATION combination)
		{
			switch (combination)
			{
				case VoronoiNoise_Native.VORONOI_COMBINATION.D0:
					return f0;

				case VoronoiNoise_Native.VORONOI_COMBINATION.D1_D0:
					return f1 - f0;

				case VoronoiNoise_Native.VORONOI_COMBINATION.D2_D0:
					return f2 - f0;
			}

			return 0;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Mod(float x, float y)
		{
			return x - y * Mathf.Floor(x / y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static float Frac(float v)
		{
			return v - Mathf.Floor(v);
		}

	}

}













