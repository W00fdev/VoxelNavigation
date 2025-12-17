using Fraktalia.Core.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Fraktalia.Utility
{
	public class NativeMesh
	{
		[ReadOnly]
		public FNativeList<Vector3> mesh_verticeArray;
		[ReadOnly]
		public FNativeList<int> mesh_triangleArray;
		[ReadOnly]
		public FNativeList<Vector2> mesh_uvArray;
		[ReadOnly]
		public FNativeList<Vector2> mesh_uv3Array;
		[ReadOnly]
		public FNativeList<Vector2> mesh_uv4Array;
		[ReadOnly]
		public FNativeList<Vector2> mesh_uv5Array;
		[ReadOnly]
		public FNativeList<Vector2> mesh_uv6Array;
		[ReadOnly]
		public FNativeList<Vector3> mesh_normalArray;
		[ReadOnly]
		public FNativeList<Vector4> mesh_tangentsArray;
		[ReadOnly]
		public FNativeList<Color> mesh_colorArray;
		[ReadOnly]
		public FNativeList<Matrix4x4> mesh_objectArray;

		public static NativeMesh CreateEmptyNativeMesh()
		{
			NativeMesh nmesh = new NativeMesh();		
			nmesh.mesh_verticeArray = new FNativeList<Vector3>(0, Allocator.Persistent);
			nmesh.mesh_triangleArray = new FNativeList<int>(0, Allocator.Persistent);
			nmesh.mesh_uvArray = new FNativeList<Vector2>(0, Allocator.Persistent);
			nmesh.mesh_uv3Array = new FNativeList<Vector2>(0, Allocator.Persistent);
			nmesh.mesh_uv4Array = new FNativeList<Vector2>(0, Allocator.Persistent);
			nmesh.mesh_uv5Array = new FNativeList<Vector2>(0, Allocator.Persistent);
			nmesh.mesh_uv6Array = new FNativeList<Vector2>(0, Allocator.Persistent);
			nmesh.mesh_normalArray = new FNativeList<Vector3>(0, Allocator.Persistent);
			nmesh.mesh_tangentsArray = new FNativeList<Vector4>(0, Allocator.Persistent);
			nmesh.mesh_colorArray = new FNativeList<Color>(0, Allocator.Persistent);
			nmesh.mesh_objectArray = new FNativeList<Matrix4x4>(0, Allocator.Persistent);
			return nmesh;
		}


		public static NativeMesh CreateNativeMesh(Mesh crystal)
		{
			NativeMesh nmesh = new NativeMesh();
			var vertices = crystal.vertices;
			var triangles = crystal.triangles;
			var uv = crystal.uv;
			var uv3 = crystal.uv3;
			var uv4 = crystal.uv4;
			var uv5 = crystal.uv5;
			var uv6 = crystal.uv6;
			var normals = crystal.normals;
			var tangents = crystal.tangents;
			var colors = crystal.colors;

			int vertcount = vertices.Length;
			int tricount = triangles.Length;

			nmesh.mesh_verticeArray = new FNativeList<Vector3>(vertcount, Allocator.Persistent);
			nmesh.mesh_triangleArray = new FNativeList<int>(tricount, Allocator.Persistent);
			nmesh.mesh_uvArray = new FNativeList<Vector2>(vertcount, Allocator.Persistent);
			nmesh.mesh_uv3Array = new FNativeList<Vector2>(0, Allocator.Persistent);
			nmesh.mesh_uv4Array = new FNativeList<Vector2>(0, Allocator.Persistent);
			nmesh.mesh_uv5Array = new FNativeList<Vector2>(0, Allocator.Persistent);
			nmesh.mesh_uv6Array = new FNativeList<Vector2>(0, Allocator.Persistent);
			nmesh.mesh_normalArray = new FNativeList<Vector3>(vertcount, Allocator.Persistent);
			nmesh.mesh_tangentsArray = new FNativeList<Vector4>(vertcount, Allocator.Persistent);
			nmesh.mesh_colorArray = new FNativeList<Color>(vertcount, Allocator.Persistent);
			nmesh.mesh_objectArray = new FNativeList<Matrix4x4>(0, Allocator.Persistent);

			for (int i = 0; i < vertcount; i++)
			{
				nmesh.mesh_verticeArray.Add(crystal.vertices[i]);
				nmesh.mesh_uvArray.Add(crystal.uv[i]);
				nmesh.mesh_normalArray.Add(crystal.normals[i]);
				nmesh.mesh_tangentsArray.Add(crystal.tangents[i]);

			}

			for (int i = 0; i < crystal.colors.Length; i++)
			{
				nmesh.mesh_colorArray.Add(crystal.colors[i]);
			}

			var array = crystal.uv3;
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					nmesh.mesh_uv3Array.Add(array[i]);
				}
			}
			array = crystal.uv4;
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					nmesh.mesh_uv4Array.Add(array[i]);
				}
			}
			array = crystal.uv5;
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					nmesh.mesh_uv5Array.Add(array[i]);
				}
			}
			array = crystal.uv6;
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					nmesh.mesh_uv6Array.Add(array[i]);
				}
			}

			for (int i = 0; i < tricount; i++)
			{
				nmesh.mesh_triangleArray.Add(crystal.triangles[i]);
			}

			return nmesh;
		}

        internal void Dispose()
        {
			if (mesh_verticeArray.IsCreated) mesh_verticeArray.Dispose();
			if (mesh_triangleArray.IsCreated) mesh_triangleArray.Dispose();
			if (mesh_uvArray.IsCreated) mesh_uvArray.Dispose();
			if (mesh_normalArray.IsCreated) mesh_normalArray.Dispose();
			if (mesh_tangentsArray.IsCreated) mesh_tangentsArray.Dispose();
			if (mesh_colorArray.IsCreated) mesh_colorArray.Dispose();
			if (mesh_uv3Array.IsCreated) mesh_uv3Array.Dispose();
			if (mesh_uv4Array.IsCreated) mesh_uv4Array.Dispose();
			if (mesh_uv5Array.IsCreated) mesh_uv5Array.Dispose();
			if (mesh_uv6Array.IsCreated) mesh_uv6Array.Dispose();
			if (mesh_objectArray.IsCreated) mesh_objectArray.Dispose();
		}
    }

	
}
