using UnityEngine;
using Fraktalia.Core.Collections;

namespace Fraktalia.VoxelGen.Modify
{
    public class VM_PostProcessBehavior : MonoBehaviour
    {
		public bool Disabled;

		public virtual void ApplyPostprocess(FNativeList<NativeVoxelModificationData_Inner> modifierData, VoxelGenerator generator, VoxelModifier_V2 modifier, VoxelModifierMode mode)
		{

		}

		public virtual void FinalizeModification(FNativeList<NativeVoxelModificationData_Inner> modifierData,
			FNativeList<NativeVoxelModificationData_Inner> preVoxelData,
			FNativeList<NativeVoxelModificationData_Inner> postVoxelData, VoxelGenerator generator, VoxelModifier_V2 modifier)
		{

		}

		public virtual void CleanUp()
		{

		}

		public virtual void DrawEditorPreview(VoxelModifier_V2 modifier, bool isSafe, Vector3 worldPosition, Vector3 worldNormal)
		{

		}
	}

}
