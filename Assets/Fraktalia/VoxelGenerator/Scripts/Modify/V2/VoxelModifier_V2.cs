using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;
using Fraktalia.Core.FraktaliaAttributes;
using Unity.Collections;
using System;
using Unity.Burst;
using Fraktalia.Core.Collections;
using Fraktalia.VoxelGen.Modify.Positioning;
using Fraktalia.Utility;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Fraktalia.VoxelGen.Modify
{
	public enum VoxelModifierMode
	{
		Set,
		Additive,
		Subtractive,
		Smooth
	}

	[ExecuteInEditMode]
	public class VoxelModifier_V2 : MonoBehaviour
	{
		public const int SAFETYLIMIT = 100000;

		[Dimension(0, 32)]
		public int TargetDimension;

		[SerializeField]
		[HideInInspector]
		public DimensionDefinitions _dimensionDefinitions;

		[Range(1, NativeVoxelTree.MaxDepth)]
		[Tooltip("Default Depth when ModifyAtPos is called")]
		public int Depth = 7;	
		public VoxelModifierMode Mode;
		[Range(0,1)]
		public float Opacity = 1;
		[Range(0, 5)]
		public float ShapeScale = 1;


		public TargetingModuleContainer TargetingModule;
		public List<TargetingModuleContainer> AdditionalTargetingModules;

		public VoxelShapeContainer ShapeModule;
		public List<VM_PostProcessContainer> PostProcessModule = new List<VM_PostProcessContainer>();
		[Header("Customizing")]
		[Tooltip("VM_PostProcessBehavior allows you to implement custom post processes by inheriting the VM_PostProcessBehavior class. These are applied AFTER the built in post processes.")]
		public List<VM_PostProcessBehavior> PostProcessModuleBehaviors = new List<VM_PostProcessBehavior>();

		[Header("Settings:")]
		[Tooltip("If true, tells the engine that it is important to know the volume BEFORE and AFTER the modification was applied. Important for processes which need the difference. This setting causes the engine to always be one frame behind the modifier.")]
		public bool RequireVoxelData;

		[Tooltip("Adds a half voxel size to the offset in order to center the result")]
		public bool MarchingCubesOffset = true;
		[Range(1, 10)][Tooltip("How many requests are processed each update. Keep at: 5")]
		public int ProcessingSpeed = 5;		
		[Tooltip("If true, modification requests are only processed when the affected voxel generator is not processing voxels. " +
			"Prevents request stacking which is important when applying modification rules but modification will lag behind. " +
			"Recommended true when using Additive/Subtractive combined with post processes (Between, Solid Paint, Hardness).")]
		public bool CleanModificationsOnly = false;
		
		[NonSerialized]
		public string ErrorMessage = "";

	
		public VoxelGenerator ReferenceGenerator
		{
			get
			{
				VoxelGenerator reference = null;
				reference = TargetingModule.Reference;

				if (!reference) reference = GetComponent<VoxelGenerator>();
				return reference;
			}
		}


		private List<VoxelGenerator> targets = new List<VoxelGenerator>();
		public int VoxelCount { get; private set; }


		private FNativeList<NativeVoxelModificationData_Inner> modifierData;
		private FNativeList<NativeVoxelModificationData_Inner> preVoxelData;
		private FNativeList<NativeVoxelModificationData_Inner> postVoxelData;

		public Queue<IEnumerator> UpdateModificationProcess = new Queue<IEnumerator>();
		IEnumerator CurrentModificationProcess;

		private RepositionSmoothJob smooth;
		private GatherInformationJob gatherpredata;
		private RepositionJob reposition;
		private OpacityJob setopacity;

		public void SetProcessingSpeed(int speed) => ProcessingSpeed = speed;
		public void SetTargetDimension(int dimension) => TargetDimension = dimension;
		public void SetTargetDimension(float dimension) => TargetDimension = (int)dimension;
		public void SetOpacity(float opacity) => Opacity = opacity;
		public void SetDepth(int depth) => Depth = depth;
		public void SetCleanModificationsOnly(bool state) => CleanModificationsOnly = state;
		public void SetShapeScale(float scale) => ShapeScale = scale;

		private void OnDrawGizmosSelected()
		{			
			VoxelGenerator reference = ReferenceGenerator;
			if (reference)
			{
				VoxelCount = ShapeModule.VoxelShape.GetVoxelModificationCount(this, reference);
				DrawEditorPreview(transform.position, Vector3.up);
				VoxelUtility_V2.DrawVoxelGeneratorBoundary(reference);
			}
		}

		private void OnDrawGizmos()
		{			
			if (!Application.isPlaying)
			{
				Update();
			}
		}

		public void DrawEditorPreview(Vector3 worldPosition, Vector3 normal)
		{
			 ShapeModule.VoxelShape.DrawEditorPreview(this, VoxelCount >= 0 && VoxelCount < SAFETYLIMIT, worldPosition, normal);

            for (int i = 0; i < PostProcessModule.Count; i++)
            {
				PostProcessModule[i].PostProcess.DrawEditorPreview(this, VoxelCount >= 0 && VoxelCount < SAFETYLIMIT, worldPosition, normal);
			}

            for (int i = 0; i < PostProcessModuleBehaviors.Count; i++)
            {
				if(PostProcessModuleBehaviors[i])
				PostProcessModuleBehaviors[i].DrawEditorPreview(this, VoxelCount >= 0 && VoxelCount < SAFETYLIMIT, worldPosition, normal);
			}
		}	

		public void ApplyVoxelModifier(Vector3 worldPosition)
		{
			EvaluateTarget(worldPosition);
			for (int i = 0; i < targets.Count; i++)
			{
				int modificationcount = ShapeModule.VoxelShape.GetVoxelModificationCount(this, targets[i]);
				if(modificationcount > 0 && modificationcount < SAFETYLIMIT)
				{
					UpdateModificationProcess.Enqueue(ModifyGenerator(targets[i], worldPosition, Mode));
				}			
			}		
		}

		public void ApplyVoxelModifier(Vector3 worldPosition, VoxelModifierMode mode)
		{
			EvaluateTarget(worldPosition);
			for (int i = 0; i < targets.Count; i++)
			{
				int modificationcount = ShapeModule.VoxelShape.GetVoxelModificationCount(this, targets[i]);
				if (modificationcount > 0 && modificationcount < SAFETYLIMIT)
				{
					UpdateModificationProcess.Enqueue(ModifyGenerator(targets[i], worldPosition, mode));
				}
			}
		}

		public void ApplyVoxelModifier()
		{
			ApplyVoxelModifier(transform.position);
		}

		public void ApplyPositioning(VoxelModifyPosition positioning)
		{
			Vector3 point = transform.position + positioning.Calculate();
			if (!positioning.NoTargetFound)
			{
				ApplyVoxelModifier(point);
			}
		}

		public Vector3 GetGameIndicatorSize()
		{			
			return ShapeModule.VoxelShape.GetGameIndicatorSize(this);
		}

		private void EvaluateTarget(Vector3 worldPosition)
		{
			targets.Clear();
			TargetingModule.FetchGenerators(targets, worldPosition, GetGameIndicatorSize());

            for (int i = 0; i < AdditionalTargetingModules.Count; i++)
            {
				AdditionalTargetingModules[i].FetchGenerators(targets, worldPosition, GetGameIndicatorSize());
			}
		}

		private IEnumerator ModifyGenerator(VoxelGenerator generator, Vector3 worldPosition, VoxelModifierMode mode)
		{
			if (generator == null || !generator.IsInitialized)
			{
				CurrentModificationProcess = null;
				yield break;
			}

			if(TargetDimension >= generator.Data.Length)
            {
				Debug.LogWarning("Target Dimension is greater than DimensionCount!", this);
				yield break;
			}

			int modificationcount = ShapeModule.VoxelShape.GetVoxelModificationCount(this, generator);
			if (!(modificationcount > 0 && modificationcount < SAFETYLIMIT))
			{
				Debug.LogWarning("Safety system kicked in! This could have crashed your game/editor", generator);
				yield break;
			}

			if (!modifierData.IsCreated) modifierData = new FNativeList<NativeVoxelModificationData_Inner>(Allocator.Persistent);
			if (!preVoxelData.IsCreated) preVoxelData = new FNativeList<NativeVoxelModificationData_Inner>(Allocator.Persistent);
			if (!postVoxelData.IsCreated) postVoxelData = new FNativeList<NativeVoxelModificationData_Inner>(Allocator.Persistent);

			

			while (generator.VoxelTreeDirty && CleanModificationsOnly)
			{
				yield return null;
			}

			generator.savesystem?.SetDirty();

			ShapeModule.VoxelShape.CreateModifierTemplate(worldPosition, this, generator);
			
			if (modifierData.Length != ShapeModule.VoxelShape.ModifierTemplateData.Length)
			{
				modifierData.Resize(ShapeModule.VoxelShape.ModifierTemplateData.Length, NativeArrayOptions.UninitializedMemory);
			}

			Vector3 worldoffset = worldPosition + ShapeModule.VoxelShape.GetOffset(this, generator);	
			Vector3Int offset = VoxelGenerator.ConvertWorldToInner(worldoffset, generator.transform.worldToLocalMatrix, generator.RootSize);

			var manifest = VoxelUndoSystem.GetManifestElement();
			if (manifest != null)
			{
				manifest.AffectedTarget = generator;
				manifest.Dimension = TargetDimension;
				VoxelUndoSystem.AddManifestElement(manifest);
			}

			if (mode == VoxelModifierMode.Smooth)
			{			 
				smooth.template = ShapeModule.VoxelShape.ModifierTemplateData;
				smooth.results = modifierData;
				smooth.Offset_X = offset.x;
				smooth.Offset_Y = offset.y;
				smooth.Offset_Z = offset.z;
				smooth.Depth = (byte)Depth;
				smooth.Opacity = Opacity;
				smooth.data = generator.Data[TargetDimension];
				smooth.BoxWidth = 1;
				smooth.InnerVoxelSize = generator.GetInnerVoxelSize(Depth);
				smooth.Schedule(ShapeModule.VoxelShape.ModifierTemplateData.Length, ShapeModule.VoxelShape.ModifierTemplateData.Length / SystemInfo.processorCount).Complete();

				if (RequireVoxelData || !Application.isPlaying)
				{

					preVoxelData.Resize(ShapeModule.VoxelShape.ModifierTemplateData.Length, NativeArrayOptions.UninitializedMemory);
					postVoxelData.Resize(ShapeModule.VoxelShape.ModifierTemplateData.Length, NativeArrayOptions.UninitializedMemory);
					
					gatherpredata.data = generator.Data[TargetDimension];
					gatherpredata.readvoxeldata = modifierData;
					gatherpredata.resultvoxeldata = preVoxelData;
					gatherpredata.Schedule(modifierData.Length, modifierData.Length / SystemInfo.processorCount).Complete();

					if (manifest != null) manifest.AddPreviousInner(gatherpredata.resultvoxeldata);

				}

				for (int i = 0; i < PostProcessModule.Count; i++)
				{
					VM_PostProcess process = PostProcessModule[i].PostProcess;
					if (!PostProcessModule[i].Disabled) process.ApplyPostprocess(modifierData, generator, this, mode);
				}

				for (int i = 0; i < PostProcessModuleBehaviors.Count; i++)
				{
					VM_PostProcessBehavior behavior = PostProcessModuleBehaviors[i];
					if (behavior)
                    {
						if (!behavior.Disabled) behavior.ApplyPostprocess(modifierData, generator, this, mode);
					}		
				}

				if (manifest != null) manifest.AddInner(modifierData);
				generator._SetVoxels_Inner(modifierData, TargetDimension);

			}
			else
			{
				VoxelUtility_V2.RepositionVoxelInformation(offset, ref ShapeModule.VoxelShape.ModifierTemplateData, ref modifierData);
				
				if (RequireVoxelData || !Application.isPlaying)
				{
					preVoxelData.Resize(ShapeModule.VoxelShape.ModifierTemplateData.Length, NativeArrayOptions.UninitializedMemory);
					postVoxelData.Resize(ShapeModule.VoxelShape.ModifierTemplateData.Length, NativeArrayOptions.UninitializedMemory);
					GatherInformationJob gatherpredata = new GatherInformationJob();
					gatherpredata.data = generator.Data[TargetDimension];
					gatherpredata.readvoxeldata = modifierData;
					gatherpredata.resultvoxeldata = preVoxelData;
					gatherpredata.Schedule(modifierData.Length, modifierData.Length / SystemInfo.processorCount).Complete();

					if (manifest != null) manifest.AddPreviousInner(gatherpredata.resultvoxeldata);
				}
	
				setopacity.template = modifierData;
				setopacity.results = modifierData;
				setopacity.Opacity = Opacity;
				if (mode == VoxelModifierMode.Subtractive)
				{
					setopacity.Opacity = -Opacity;
				}
				setopacity.Schedule(ShapeModule.VoxelShape.ModifierTemplateData.Length, ShapeModule.VoxelShape.ModifierTemplateData.Length / SystemInfo.processorCount).Complete();

				for (int i = 0; i < PostProcessModule.Count; i++)
				{
					VM_PostProcess process = PostProcessModule[i].PostProcess;
					if(!PostProcessModule[i].Disabled) process.ApplyPostprocess(modifierData, generator, this, mode);
				}

				for (int i = 0; i < PostProcessModuleBehaviors.Count; i++)
				{
					VM_PostProcessBehavior behavior = PostProcessModuleBehaviors[i];
					if (behavior)
					{
						if (!behavior.Disabled) behavior.ApplyPostprocess(modifierData, generator, this, mode);
					}
				}

				if (manifest != null) manifest.AddInner(modifierData);
				
				if (mode == VoxelModifierMode.Additive || mode == VoxelModifierMode.Subtractive)
				{
					if (manifest != null) manifest.Additive = true;
					generator._SetVoxelsAdditive_Inner(modifierData, TargetDimension);
				}
				if (mode == VoxelModifierMode.Set)
				{
					generator._SetVoxels_Inner(modifierData, TargetDimension);
				}
			}

			ShapeModule.VoxelShape.SetGeneratorDirty(this, generator, worldPosition);

			if (RequireVoxelData)
			{
				while (generator.VoxelTreeDirty)
				{
					yield return null;
				}

				gatherpredata.data = generator.Data[TargetDimension];
				gatherpredata.readvoxeldata = modifierData;
				gatherpredata.resultvoxeldata = postVoxelData;
				gatherpredata.Schedule(modifierData.Length, modifierData.Length / SystemInfo.processorCount).Complete();
			}

			for (int i = 0; i < PostProcessModule.Count; i++)
			{
				VM_PostProcess process = PostProcessModule[i].PostProcess;
				if (!PostProcessModule[i].Disabled) process.FinalizeModification(modifierData, preVoxelData, postVoxelData, generator, this);	
			}

			for (int i = 0; i < PostProcessModuleBehaviors.Count; i++)
			{
				VM_PostProcessBehavior behavior = PostProcessModuleBehaviors[i];
				if (behavior)
				{
					if (!behavior.Disabled) behavior.FinalizeModification(modifierData, preVoxelData, postVoxelData, generator, this);
				}
			}

			CurrentModificationProcess = null;
		}

		public void CleanUp()
		{
			if (modifierData.IsCreated) modifierData.Dispose();
			if (preVoxelData.IsCreated) preVoxelData.Dispose();
			if (postVoxelData.IsCreated) postVoxelData.Dispose();

			ShapeModule.CleanUp();
			CurrentModificationProcess = null;
			UpdateModificationProcess.Clear();

			for (int i = 0; i < PostProcessModuleBehaviors.Count; i++)
			{
				VM_PostProcessBehavior behavior = PostProcessModuleBehaviors[i];
				if (behavior)
				{
					behavior.CleanUp();
				}
			}
		}

		public static void CleanAll()
		{
			VoxelModifier_V2[] shapes = GameObject.FindObjectsOfType<VoxelModifier_V2>(true);
			for (int i = 0; i < shapes.Length; i++)
			{
				shapes[i].CleanUp();
			}		
		}

		private void OnDestroy()
		{
			CleanUp();
		}

		public bool SafetyCheck()
		{
			ErrorMessage = "";
			bool isSave = true;		

			VoxelGenerator reference = ReferenceGenerator;
			if(reference)
			{
				int modificationcount = ShapeModule.VoxelShape.GetVoxelModificationCount(this, reference);
				if (!(modificationcount > 0 && modificationcount < SAFETYLIMIT))
				{
					ErrorMessage += "> This setting would modify more than 100000 voxels on the reference generaor: " + reference +
						". Modifier will not be applied on this generator but can still be applied on other generators which are deemed safe!\n\n";

					isSave = false;
				}
			}
			
			return isSave;
		}

		private void Update()
		{
			for (int i = 0; i < ProcessingSpeed; i++)
			{
				if (CurrentModificationProcess == null)
				{
					if (UpdateModificationProcess.Count > 0)
						CurrentModificationProcess = UpdateModificationProcess.Dequeue();
				}

				if (CurrentModificationProcess != null)
				{	
					CurrentModificationProcess.MoveNext();				
				}
			}
		}

		public bool HasPostProcess<T>() where T : VM_PostProcess
		{
			foreach (var item in PostProcessModule)
			{
				if (item.PostProcess is T) return true;
			}

			return false;
		}

		public T GetPostProcess<T>() where T : VM_PostProcess
		{
			foreach (var item in PostProcessModule)
			{
				if (item.PostProcess is T) return (T)item.PostProcess;
			}

			return null;
		}
	}

	[BurstCompile]
	public struct RepositionJob : IJobParallelFor
	{
		public int Offset_X;
		public int Offset_Y;
		public int Offset_Z;
		
		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> template;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> results;


		public void Execute(int index)
		{
			NativeVoxelModificationData_Inner result = template[index];
			result.X = result.X + Offset_X;
			result.Y = result.Y + Offset_Y;
			result.Z = result.Z + Offset_Z;
			results[index] = result;
		}
	}

	[BurstCompile]
	public struct OpacityJob : IJobParallelFor
	{
		public float Opacity;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> template;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> results;


		public void Execute(int index)
		{
			NativeVoxelModificationData_Inner result = template[index];
			result.ID = (int)(result.ID * Opacity);
			results[index] = result;
		}
	}

	[BurstCompile]
	public struct RepositionSmoothJob : IJobParallelFor
	{
		[NativeDisableParallelForRestriction]
		public NativeVoxelTree data;
		public int BoxWidth;
		public int InnerVoxelSize;

		public int Offset_X;
		public int Offset_Y;
		public int Offset_Z;
		public byte Depth;
		public float Opacity;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> template;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> results;

		
		public void Execute(int index)
		{
			NativeVoxelModificationData_Inner result = template[index];
			result.X = result.X + Offset_X;
			result.Y = result.Y + Offset_Y;
			result.Z = result.Z + Offset_Z;
			result.Depth = Depth;
			int TargetID = (int)(result.ID * Opacity);

			int FinalValue = data._PeekVoxelId_InnerCoordinate(result.X, result.Y, result.Z, 20, 0, 128);
			int PeekedID = 0;
			int count = 0;
			for (int a = -BoxWidth; a <= BoxWidth; a++)
			{
				for (int b = -BoxWidth; b <= BoxWidth; b++)
				{
					for (int c = -BoxWidth; c <= BoxWidth; c++)
					{
						int fx = result.X + a * InnerVoxelSize;
						int fy = result.Y + b * InnerVoxelSize;
						int fz = result.Z + c * InnerVoxelSize;

						int Value = data._PeekVoxelId_InnerCoordinate(fx, fy, fz, 20, 0, 128);
						PeekedID += Value;
						count++;
					}
				}
			}

			PeekedID /= count;

			if (FinalValue > PeekedID)
			{
				FinalValue = Mathf.Max(PeekedID, FinalValue - TargetID);
			}
			else if (FinalValue < PeekedID)
			{
				FinalValue = Mathf.Min(PeekedID, FinalValue + TargetID);
			}

			result.ID = FinalValue;



			results[index] = result;
		}
	}

	[BurstCompile]
	public struct GatherInformationJob : IJobParallelFor
	{
		public NativeVoxelTree data;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> readvoxeldata;

		[NativeDisableContainerSafetyRestriction]
		public FNativeList<NativeVoxelModificationData_Inner> resultvoxeldata;

		public void Execute(int index)
		{
			NativeVoxelModificationData_Inner modifier = readvoxeldata[index];
			if (modifier.ID != -1)
			{
				int Value = data._PeekVoxelId_InnerCoordinate(modifier.X, modifier.Y, modifier.Z, 20, 0, -1);
				modifier.ID = Value;
			}
			resultvoxeldata[index] = modifier;
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(VoxelModifier_V2))]
	[CanEditMultipleObjects]
	public class VoxelModifier_V2Editor : Editor
	{
		UnityEditor.SceneView sv;

		private void OnSceneGUI()
		{
			VoxelModifier_V2 myTarget = target as VoxelModifier_V2;

			var e = Event.current;
			if (e.type == EventType.KeyDown)
			{

				if (e.keyCode == VoxelGeneratorSettings.ApplyKey)
				{
					VoxelUndoSystem.CreateManifest();					
					myTarget.ApplyVoxelModifier(myTarget.transform.position);
					e.Use();
					
				}

				if (e.keyCode == VoxelGeneratorSettings.UndoKey)
				{
					VoxelUndoSystem.Undo();
				}

				if (e.keyCode == VoxelGeneratorSettings.RedoKey)
				{
					VoxelUndoSystem.Redo();
				}
			}

			if(e.type == EventType.KeyUp)
			{
				if (e.keyCode == VoxelGeneratorSettings.ApplyKey)
				{
					VoxelUndoSystem.FinishManifest();				
					e.Use();			
				}
			}
		}	

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

			GUIStyle normal = new GUIStyle();
			normal.fontStyle = FontStyle.Normal;
			normal.fontSize = 12;
			normal.richText = true;

			VoxelModifier_V2 myTarget = target as VoxelModifier_V2;

			if (!Application.isPlaying)
			{
				if (sv == null) sv = UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>();
				if (sv != null)
				{
					if (!sv.drawGizmos)
					{
						EditorGUILayout.Space();
						EditorGUILayout.LabelField("<color=red>GIZMOS ARE DISABLED!</color>", title);
						EditorGUILayout.TextArea("Sculpting in edit mode not possible if Gizmos is disabled!");

						if (GUILayout.Button("Enable Gizmos"))
						{
							sv.drawGizmos = true;
						}
					}
				}
			}

			DrawDefaultInspector();
			
			if (!myTarget.SafetyCheck())
			{
				EditorGUILayout.LabelField("<color=red>Errors:</color>", bold);
				EditorGUILayout.TextArea(myTarget.ErrorMessage);
			}
			EditorGUILayout.LabelField("Voxel Count:" + myTarget.VoxelCount);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Editing Tools:", bold);
			if (GUILayout.Button("Apply at world position"))
			{
				VoxelUndoSystem.CreateManifest();
				for (int i = 0; i < targets.Length; i++)
				{
					var current = targets[i] as VoxelModifier_V2;
					if (current)
					{
						current.ApplyVoxelModifier(current.transform.position);					
					}
				}	
				VoxelUndoSystem.FinishManifest();
			}

			VoxelModifyPosition pos = myTarget.GetComponent<VoxelModifyPosition>();
			if (pos)
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Apply Positioning"))
				{
					myTarget.ApplyPositioning(pos);
				}

				if (GUILayout.Button("Apply Positioning * 10"))
				{
					for (int i = 0; i < 10; i++)
					{
						myTarget.ApplyPositioning(pos);
					}

				}

				if (GUILayout.Button("Apply Positioning * 100"))
				{
					for (int i = 0; i < 100; i++)
					{
						myTarget.ApplyPositioning(pos);
					}
				}		
				EditorGUILayout.EndHorizontal();
			}


			if (!myTarget.GetComponent<VoxelModifier_V2_Raycaster>())
			{
				EditorGUILayout.LabelField("Voxel Raycaster is required for editor painting:");
				if (GUILayout.Button("Add raycaster"))
					myTarget.gameObject.AddComponent<VoxelModifier_V2_Raycaster>();
			}

			VoxelGeneratorSettings.DisplaySettingsInfo("Apply Hotkey: " , VoxelGeneratorSettings.ApplyKey.ToString(), normal);
			VoxelGeneratorSettings.DisplaySettingsInfo("Undo Hotkey: ", VoxelGeneratorSettings.UndoKey.ToString(), normal);
			VoxelGeneratorSettings.DisplaySettingsInfo("Redo Hotkey: ", VoxelGeneratorSettings.RedoKey.ToString(), normal);

			if (GUI.changed)
			{			
				serializedObject.ApplyModifiedProperties();
				myTarget.ShapeModule.ConvertToDerivate();
				
                foreach (var item in targets)
                {
					myTarget = item as VoxelModifier_V2;
					if (myTarget != null)
					{
						myTarget.ShapeModule.ConvertToDerivate();
						myTarget.TargetingModule.ConvertToDerivate();
						for (int i = 0; i < myTarget.PostProcessModule.Count; i++)
						{
							myTarget.PostProcessModule[i].ConvertToDerivate();
						}

						for (int i = 0; i < myTarget.AdditionalTargetingModules.Count; i++)
						{
							myTarget.AdditionalTargetingModules[i].ConvertToDerivate();
						}					
					}
				}


				EditorUtility.SetDirty(target);
			}	
		}

		
	}

	

#endif
}
