using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Fraktalia.Core.FraktaliaAttributes;
using Fraktalia.Core.Collections;
using Fraktalia.Core.LMS;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;

#endif

namespace Fraktalia.VoxelGen.Modify
{
	[System.Serializable]
	public class VM_PostProcess_Destruction : VM_PostProcess
	{		
		public Transform DestructionPrefab;
		public int MinimumMaterialID = 128;
		public int RequiredSpawnMaterial = 1000;
	
		public int RemovedMaterial = 0;
		public MeshPieceAttachment[] Attachments = new MeshPieceAttachment[0];

		private bool usesPreVoxelData;

        public override void ApplyPostprocess(FNativeList<NativeVoxelModificationData_Inner> modifierData, VoxelGenerator generator, VoxelModifier_V2 modifier, VoxelModifierMode mode)
        {
			usesPreVoxelData = modifier.RequireVoxelData;
        }

        public override void FinalizeModification(FNativeList<NativeVoxelModificationData_Inner> modifierData, FNativeList<NativeVoxelModificationData_Inner> preVoxelData, FNativeList<NativeVoxelModificationData_Inner> postVoxelData, VoxelGenerator generator, VoxelModifier_V2 modifier)
		{
			if (!Application.isPlaying) return;
			if (DestructionPrefab == null) return;
			if (RemovedMaterial < 0) RemovedMaterial = 0;

			if (usesPreVoxelData)
			{
				for (int i = 0; i < preVoxelData.Length; i++)
				{
					int preModifyID = preVoxelData[i].ID;
					if (preModifyID >= MinimumMaterialID)
					{

						var data = postVoxelData[i];
						int postModifyID = data.ID;
						int difference = preModifyID - postModifyID;

						RemovedMaterial += difference;
						if (RemovedMaterial > RequiredSpawnMaterial)
						{
							Transform newobject = GameObject.Instantiate<Transform>(DestructionPrefab);
							newobject.transform.position = VoxelGenerator.ConvertInnerToWorld(new Vector3Int(data.X, data.Y, data.Z), generator.transform.localToWorldMatrix, generator.RootSize);
							newobject.transform.localScale = Vector3.one * generator.GetVoxelSize(data.Depth);

							RemovedMaterial = 0;

							for (int k = 0; k < Attachments.Length; k++)
							{
								var attachment = Attachments[k];
								if (attachment)
									attachment.Effect(newobject.gameObject);
							}
						}
					}
				}
			}
            else
            {
				for (int i = 0; i < modifierData.Length; i++)
				{
					var data = modifierData[i];
					int preModifyID = data.ID * -1;
				
					if (preModifyID >= MinimumMaterialID)
					{				
						int postModifyID = data.ID;
						int difference = Mathf.Abs(preModifyID);

						RemovedMaterial += difference;
						if (RemovedMaterial > RequiredSpawnMaterial)
						{
							Transform newobject = GameObject.Instantiate<Transform>(DestructionPrefab);
							newobject.transform.position = VoxelGenerator.ConvertInnerToWorld(new Vector3Int(data.X, data.Y, data.Z), generator.transform.localToWorldMatrix, generator.RootSize);
							newobject.transform.localScale = Vector3.one * generator.GetVoxelSize(data.Depth);

							RemovedMaterial = 0;

							for (int k = 0; k < Attachments.Length; k++)
							{
								var attachment = Attachments[k];
								if (attachment)
									attachment.Effect(newobject.gameObject);
							}
						}
					}
				}
			}


		}
	}

/*
#if UNITY_EDITOR
	[CanEditMultipleObjects]
	[CustomEditor(typeof(VM_PostProcess_Destruction))]
	public class VM_PostProcess_DestructionEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			GUIStyle title = new GUIStyle();
			title.fontStyle = FontStyle.Bold;
			title.fontSize = 14;
			title.richText = true;

			GUIStyle bold = new GUIStyle();
			bold.fontStyle = FontStyle.Bold;
			bold.fontSize = 12;
			bold.richText = true;


			EditorStyles.textField.wordWrap = true;



			VM_PostProcess_Destruction myTarget = (VM_PostProcess_Destruction)target;




			DrawDefaultInspector();

			EditorGUILayout.LabelField("Attachments: " + myTarget.GetComponents<MeshPieceAttachment>().Length);
			var attachments = FraktaliaEditorUtility.GetDerivedTypesForScriptSelection(typeof(MeshPieceAttachment), "Add Mesh Attachment...");
			int selectedattachments = EditorGUILayout.Popup(0, attachments.Item2);
			if (selectedattachments > 0)
			{
				myTarget.gameObject.AddComponent(attachments.Item1[selectedattachments]);
				myTarget.Attachments = myTarget.gameObject.GetComponents<MeshPieceAttachment>();
				EditorUtility.SetDirty(myTarget);
			}

		}
	}
#endif
*/

}
