using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Fraktalia.Core.FraktaliaAttributes;
using Fraktalia.Core.Collections;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;

#endif

namespace Fraktalia.VoxelGen.Modify
{
	[System.Serializable]
	public class VM_PostProcess_CopyPaste : VM_PostProcess
	{
		public VoxelGenerator TargetGenerator;
		[Range(0, 5)]
		public int CopyDimension;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> copyinformation;

		[Button("Create Copy at transform")]
		public bool _copyInformation;

		private int _datacopiedFrames;
		private Vector3 _copypoint; 

		public override void ApplyPostprocess(FNativeList<NativeVoxelModificationData_Inner> modifierData, VoxelGenerator generator, VoxelModifier_V2 modifier, VoxelModifierMode mode)
		{
			if (!copyinformation.IsCreated)
			{
				Debug.LogError("No copy information created!");
				modifierData.Clear();
				return;
			}

			if (copyinformation.Length != modifierData.Length)
			{
				Debug.LogError("Copy data does not match modifier Data.");
				modifierData.Clear();
				return;
			}

			CopyPasteJob job = new CopyPasteJob();
			job.copyData = copyinformation;
			job.modifierData = modifierData;
			job.Schedule(modifierData.Length, modifierData.Length / SystemInfo.processorCount).Complete();
		}
		
		public void CopyVoxelData(VoxelModifier_V2 modifier, Vector3 worldPosition)
		{
			if (modifier == null) return;

			modifier.ShapeModule.VoxelShape.CreateModifierTemplate(worldPosition, modifier, TargetGenerator);
			Vector3 worldoffset = worldPosition + modifier.ShapeModule.VoxelShape.GetOffset(modifier, TargetGenerator);
			Vector3Int offset = VoxelGenerator.ConvertWorldToInner(worldoffset, TargetGenerator.transform.worldToLocalMatrix, TargetGenerator.RootSize);

			if (!copyinformation.IsCreated) copyinformation = new FNativeList<NativeVoxelModificationData_Inner>(Allocator.Persistent);

			VoxelUtility_V2.RepositionVoxelInformation(offset, ref modifier.ShapeModule.VoxelShape.ModifierTemplateData, ref copyinformation);
			VoxelUtility_V2.GatherVoxelInformation(ref TargetGenerator.Data[CopyDimension], ref copyinformation, ref copyinformation);

			_datacopiedFrames = 30;
			_copypoint = worldPosition;
		}

		public override void CleanUp()
		{
			if (copyinformation.IsCreated) copyinformation.Dispose();
			base.CleanUp();
		}

        public override void DrawEditorPreview(VoxelModifier_V2 modifier, bool isSafe, Vector3 worldPosition, Vector3 worldNormal)
        {
			if (_copyInformation)
			{
				_copyInformation = false;
				CopyVoxelData(modifier, modifier.transform.position);		
			}

			if(_datacopiedFrames > 0)
            {
				_datacopiedFrames--;	
				Gizmos.matrix = Matrix4x4.TRS(_copypoint, Quaternion.identity, modifier.ShapeModule.VoxelShape.GetGameIndicatorSize(modifier));
				Gizmos.color = new Color32(0, 255, 0, 255);
				Gizmos.DrawSphere(Vector3.zero, 0.35f);
			}

			if(copyinformation.IsCreated)
            {
				Gizmos.matrix = Matrix4x4.TRS(modifier.transform.position, Quaternion.identity, modifier.ShapeModule.VoxelShape.GetGameIndicatorSize(modifier));
				Gizmos.color = new Color32(0, 255, 0, 64);
				Gizmos.DrawSphere(Vector3.zero, 0.35f);
			}
		}
    
		public static bool HasCopyModule(VoxelModifier_V2 modifier)
        {
            for (int i = 0; i < modifier.PostProcessModule.Count; i++)
            {
				if (modifier.PostProcessModule[i].PostProcess is VM_PostProcess_CopyPaste) return true;
            }

			return false;
        }
	}

	[BurstCompile]
	public struct CopyPasteJob : IJobParallelFor
	{
		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> modifierData;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> copyData;

		public void Execute(int index)
		{
			NativeVoxelModificationData_Inner modifier = modifierData[index];
			NativeVoxelModificationData_Inner copydata = copyData[index];
			modifier.ID = (int)(copydata.ID * (float)modifier.ID / 256.0f);
			modifierData[index] = modifier;
		}
	}

}