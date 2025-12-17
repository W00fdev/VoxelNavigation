using Fraktalia.Core.Collections;
using Fraktalia.Core.Math;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace Fraktalia.VoxelGen.Modify
{

	[System.Serializable]
	public class VoxelShape_Single : VoxelShape_Base
	{
		public int InitialID = 255;		
		private float offset;
		private SingleCalculationJob job;

		public override void DrawEditorPreview(VoxelModifier_V2 modifier, bool isSafe, Vector3 worldPosition, Vector3 normal)
		{
			if (modifier.ReferenceGenerator == null) return;

			float voxelsize = modifier.ReferenceGenerator.GetVoxelSize(modifier.Depth);


			if (!isSafe)
			{
				Gizmos.color = Color.red;
			}
			Gizmos.color = new Color32(150, 150, 255, 200);

			Vector3 singlePos = worldPosition - normal * voxelsize * 0.1f;
			singlePos.x -= (singlePos.x % voxelsize) - voxelsize / 2;
			singlePos.y -= (singlePos.y % voxelsize) + voxelsize / 2;
			singlePos.z -= (singlePos.z % voxelsize) - voxelsize / 2;

			//Vector3Int innerPos = VoxelGenerator.ConvertWorldToInner(singlePos, modifier.ReferenceGenerator.transform.worldToLocalMatrix, modifier.ReferenceGenerator.RootSize);
			//singlePos = VoxelGenerator.ConvertInnerToWorld(innerPos, modifier.ReferenceGenerator.transform.localToWorldMatrix, modifier.ReferenceGenerator.RootSize);


			Gizmos.matrix = Matrix4x4.TRS(singlePos, Quaternion.identity, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(voxelsize, voxelsize, voxelsize));
			Gizmos.matrix = Matrix4x4.identity;

			singlePos = modifier.transform.position;
			singlePos.x -= (singlePos.x % voxelsize) - voxelsize / 2;
			singlePos.y -= (singlePos.y % voxelsize) + voxelsize / 2;
			singlePos.z -= (singlePos.z % voxelsize) - voxelsize / 2;

			//innerPos = VoxelGenerator.ConvertWorldToInner(singlePos, modifier.ReferenceGenerator.transform.worldToLocalMatrix, modifier.ReferenceGenerator.RootSize);
			//singlePos = VoxelGenerator.ConvertInnerToWorld(innerPos, modifier.ReferenceGenerator.transform.localToWorldMatrix, modifier.ReferenceGenerator.RootSize);

			Gizmos.matrix = Matrix4x4.TRS(singlePos, Quaternion.identity, Vector3.one);
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(voxelsize, voxelsize, voxelsize));
			Gizmos.matrix = Matrix4x4.identity;
		}

		public override Vector3 GetGameIndicatorSize(VoxelModifier_V2 modifier)
		{
			float voxelsize = 1;
			if (modifier.ReferenceGenerator != null)
				voxelsize = modifier.ReferenceGenerator.GetVoxelSize(modifier.Depth);

			return new Vector3(voxelsize, voxelsize, voxelsize);
		}

		protected override void calculateTemplateData(VoxelModifier_V2 modifier, VoxelGenerator target)
		{		
			float voxelsize = target.GetVoxelSize(modifier.Depth);
				
			offset = voxelsize *0.5f;
			float maddition = 0;
			if (modifier.MarchingCubesOffset)
			{
				maddition = voxelsize * 0.5f;
			}
			Vector3 calculationOffset = Vector3.one * (offset - voxelsize * 0.5f + maddition) + displacement;
				
			job.voxelsize = voxelsize;
			job.innervoxelsize = target.GetInnerVoxelSize(modifier.Depth);
			job.depth = (byte)modifier.Depth;
			job.initialID = InitialID;
			job.offset = calculationOffset;
			job.template = ModifierTemplateData;

			int totalvoxels = 1;
			if (ModifierTemplateData.Length != totalvoxels)
			{
				ModifierTemplateData.Resize(totalvoxels, NativeArrayOptions.UninitializedMemory);
			}
			
			job.Schedule().Complete();	
		}

		public override void SetGeneratorDirty(VoxelModifier_V2 modifier, VoxelGenerator target, Vector3 worldPosition)
		{
			float voxelsize = target.GetVoxelSize(modifier.Depth);

			target.SetRegionsDirty(target.transform.worldToLocalMatrix.MultiplyPoint3x4(worldPosition), Vector3.one * (voxelsize * boundaryExtension/2), Vector3.one * (voxelsize * boundaryExtension / 2), modifier.TargetDimension);
		}

		public override Vector3 GetOffset(VoxelModifier_V2 modifier, VoxelGenerator target)
		{
			return -Vector3.zero * (offset);
		}

		public override int GetVoxelModificationCount(VoxelModifier_V2 modifier, VoxelGenerator target)
		{		
			return 1;
		}
	}

	[BurstCompile]
	public struct SingleCalculationJob : IJob
	{
		public float radius;		
		public int rows;
		public Vector3 offset;
		public int innervoxelsize;
		public float voxelsize;
		public int initialID;
		public byte depth;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> template;
		

		public void Execute()
		{
			NativeVoxelModificationData_Inner result = template[0];
			Vector3Int position = MathUtilities.Convert1DTo3D(0, rows, rows, rows);
			result.X = position.x * innervoxelsize;
			result.Y = position.y * innervoxelsize;
			result.Z = position.z * innervoxelsize;
			result.Depth = depth;

			Vector3 p = new Vector3(position.x * voxelsize, position.y * voxelsize, position.z * voxelsize);

			float Pos_X = p.x;
			float Pos_Y = p.y;
			float Pos_Z = p.z;

			

			float Dist_X = Pos_X - offset.x;
			float Dist_Y = Pos_Y - offset.y;
			float Dist_Z = Pos_Z - offset.z;
		
			int ID = (int)(initialID);
			result.ID = Mathf.Clamp(ID, 0, 255);
			
			template[0] = result;
		}
	}
}
