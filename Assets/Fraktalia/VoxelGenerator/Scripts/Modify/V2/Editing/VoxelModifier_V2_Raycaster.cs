using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Fraktalia.VoxelGen.Modify
{
	[System.Serializable]
	public class VoxelModifier_V2_AdditionalModifierEntry
    {
		public VoxelModifier_V2 Modifier;
		[Tooltip("If true, additive mode becomes subtractive, subtractive mode becomes additive.")]
		public bool InvertModeOverride;

	}

	public class VoxelModifier_V2_Raycaster : MonoBehaviour
	{
		public VoxelModifier_V2 Modifier;
		public List<VoxelModifier_V2_AdditionalModifierEntry> AdditionalModifiers = new List<VoxelModifier_V2_AdditionalModifierEntry>();
		

		[Header("Game Settings:")]
		public bool Active = true;
		public Camera GameCamera;
		public Transform ImpactIndicator;
		public LayerMask TargetLayer = -1;
		public float MaximumDistance;
		public KeyCode ActivationButton = KeyCode.LeftControl;

		[Header("Editor Settings:")]
		public bool ReliefPainting = false;
		public bool Paint2D = false;
		public bool PaintPlaneXY = false;
		public bool PaintPlaneXZ = false;
		public bool PaintPlaneYZ = false;
		public float PaintPlaneOffset = 0;
		public float PaintPlaneSize = 2000;
		public Color PaintPlaneColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		public Vector3 PaintPosition;
		[HideInInspector]
		public Vector3 PaintNormal;


		[Header("Interaction Settings:")]
		public VoxelModifierMode ModeLeftClick = VoxelModifierMode.Additive;
		public VoxelModifierMode ModeRightClick = VoxelModifierMode.Subtractive;
		public VoxelModifierMode ModeMiddleClick = VoxelModifierMode.Smooth;

		public float NormalOffset_Left;
		public float NormalOffset_Right;
		public float NormalOffset_Middle;




		private void OnDrawGizmosSelected()
		{
			if (Modifier) Modifier.DrawEditorPreview(PaintPosition, PaintNormal);
			else Modifier = GetComponent<VoxelModifier_V2>();

            for (int i = 0; i < AdditionalModifiers.Count; i++)
            {
				var modifier = AdditionalModifiers[i].Modifier;
				if (modifier)
				{				
					modifier.DrawEditorPreview(PaintPosition, PaintNormal);
					if(modifier.ReferenceGenerator)
                    {
						VoxelUtility_V2.DrawVoxelGeneratorBoundary(modifier.ReferenceGenerator, new Color32(255,255,255,255), 1.001f);
                    }
				}
			}
		}

		private void Update()
		{
			if (!Modifier || !GameCamera) return;
			if (!Active) return;

			if (Input.GetKey(ActivationButton) || ActivationButton == KeyCode.None)
			{
				
				Ray ray = GameCamera.ScreenPointToRay(Input.mousePosition);
				if (RayCastToVoxel(ray, MaximumDistance, TargetLayer))
				{
					if (ImpactIndicator)
					{
						ImpactIndicator.position = PaintPosition;
						ImpactIndicator.gameObject.SetActive(true);
						ImpactIndicator.localScale = Modifier.GetGameIndicatorSize();
					}
					if(Input.GetMouseButton(0))
					{
						PaintPosition += PaintNormal * NormalOffset_Left;
						ApplyModifier(ModeLeftClick);
					}

					if (Input.GetMouseButton(1))
					{
						PaintPosition += PaintNormal * NormalOffset_Right;
						ApplyModifier(ModeRightClick);
					}

					if (Input.GetMouseButton(2))
					{
						PaintPosition += PaintNormal * NormalOffset_Middle;
						ApplyModifier(ModeMiddleClick);
					}
				}
				else
				{
					if (ImpactIndicator) ImpactIndicator.gameObject.SetActive(false);
				}
			}
			else
			{
				if (ImpactIndicator) ImpactIndicator.gameObject.SetActive(false);
			}

		}


		public bool RayCastToVoxel(Ray ray, float distance, LayerMask mask)
		{
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, distance, mask.value))
			{
				PaintPosition = hit.point;
				PaintNormal = hit.normal;			
				return true;
			}
			return false;
		}

		public bool RayCastToSurface(Ray ray, Plane surface, float distance)
		{
			float enter;
			if (surface.Raycast(ray, out enter))
			{
				PaintPosition = ray.GetPoint(enter);
				PaintNormal = surface.normal;
				return true;
			}
			return false;
		}

		public void ApplyModifier()
		{
			if (Modifier) Modifier.ApplyVoxelModifier(PaintPosition);
			else Modifier = GetComponent<VoxelModifier_V2>();
			for (int i = 0; i < AdditionalModifiers.Count; i++)
			{
				if (AdditionalModifiers[i].Modifier) AdditionalModifiers[i].Modifier.ApplyVoxelModifier(PaintPosition);
			}
		}

		public void ApplyModifier(VoxelModifierMode modeoverride)
		{	
			if (Modifier)
			{
				var mode = Modifier.Mode;
				Modifier.Mode = modeoverride;
				Modifier.ApplyVoxelModifier(PaintPosition);
				Modifier.Mode = mode;
			}
			else Modifier = GetComponent<VoxelModifier_V2>();
			for (int i = 0; i < AdditionalModifiers.Count; i++)
			{
				var modifier = AdditionalModifiers[i].Modifier;
				if (modifier)
				{
					var mode = modifier.Mode;
					modifier.Mode = modeoverride;
					if(AdditionalModifiers[i].InvertModeOverride && modeoverride == VoxelModifierMode.Additive)
                    {
						modifier.Mode = VoxelModifierMode.Subtractive;
					}

					if (AdditionalModifiers[i].InvertModeOverride && modeoverride == VoxelModifierMode.Subtractive)
					{
						modifier.Mode = VoxelModifierMode.Additive;
					}

					modifier.ApplyVoxelModifier(PaintPosition);
					modifier.Mode = mode;
				}
			}
		}

		public void ApplyModifierAtTransformPoint()
        {
			var point = PaintPosition;
			PaintPosition = transform.position;
			ApplyModifier();
			PaintPosition = point;
        }

		public void ApplyModifierAtTransformPoint(VoxelModifierMode modeoverride)
		{
			var point = PaintPosition;
			PaintPosition = transform.position;
			ApplyModifier(modeoverride);
			PaintPosition = point;
		}

		public void CopyAtTransformPoint()
        {
			var postprocess = Modifier.GetPostProcess<VM_PostProcess_CopyPaste>();
			if (postprocess != null) postprocess.CopyVoxelData(Modifier, transform.position);
			
			for (int i = 0; i < AdditionalModifiers.Count; i++)
			{
				postprocess = AdditionalModifiers[i].Modifier.GetPostProcess<VM_PostProcess_CopyPaste>();
				if (postprocess != null) postprocess.CopyVoxelData(AdditionalModifiers[i].Modifier, transform.position);
			}	
		}

        public void CopyAtPaintPosition()
        {
			var postprocess = Modifier.GetPostProcess<VM_PostProcess_CopyPaste>();
			if (postprocess != null) postprocess.CopyVoxelData(Modifier, PaintPosition);

			for (int i = 0; i < AdditionalModifiers.Count; i++)
			{
				postprocess = AdditionalModifiers[i].Modifier.GetPostProcess<VM_PostProcess_CopyPaste>();
				if (postprocess != null) postprocess.CopyVoxelData(AdditionalModifiers[i].Modifier, PaintPosition);
			}
		}
    }

#if UNITY_EDITOR
	[CustomEditor(typeof(VoxelModifier_V2_Raycaster))]
	[CanEditMultipleObjects]
	public class VoxelModifier_V2_PainterEditor : Editor
	{
		int activePaintTime;
		bool ispainting = false;
		Vector3 reliefPosition;
		bool mousebuttonpressed = false;

		void OnSceneGUI()
		{
			if (Application.isPlaying) return;
			VoxelModifier_V2_Raycaster mytarget = target as VoxelModifier_V2_Raycaster;
			if (!mytarget.Modifier) return;
			Event e = Event.current;
			switch (e.type)
			{
				case EventType.KeyDown:
					{
						if (Event.current.keyCode == VoxelGeneratorSettings.PaintingKey)
						{
						
							ispainting = true;
						}
					
						break;
					}
				case EventType.KeyUp:
					{
						if (Event.current.keyCode == VoxelGeneratorSettings.PaintingKey)
						{
							ispainting = false;

						}
						
						break;
					}
			}

			if(e.type == EventType.MouseDown)
			{
				mousebuttonpressed = true;
			}
			if (e.type == EventType.MouseUp)
			{
				mousebuttonpressed = false;
			}


			if (!ispainting) return;
			HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

			RaycastHit hit = new RaycastHit();

			Vector2 guiPosition = Event.current.mousePosition;
			Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);

			Plane surface = new Plane();

			bool hastarget = false;

			
			if (mytarget.Paint2D || mytarget.ReliefPainting)
			{
				Vector3 direction = new Vector3();
				if (mytarget.PaintPlaneXY) direction.z = 1;
				if (mytarget.PaintPlaneXZ) direction.y = 1;
				if (mytarget.PaintPlaneYZ) direction.x = 1;

				direction = mytarget.transform.localToWorldMatrix.MultiplyVector(direction);
				

				if(mytarget.ReliefPainting)
				{
					if (!mousebuttonpressed)
					{
						if (Physics.Raycast(ray, out hit, 2000))
						{
							reliefPosition = hit.point;
						}
					}
					surface = new Plane(direction, reliefPosition + direction * mytarget.PaintPlaneOffset);

					Handles.DrawWireDisc(reliefPosition + direction * mytarget.PaintPlaneOffset, direction, mytarget.PaintPlaneSize);
					Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
					Handles.color = mytarget.PaintPlaneColor;
					Handles.DrawSolidDisc(reliefPosition + direction * mytarget.PaintPlaneOffset, direction, mytarget.PaintPlaneSize);
				}
				else
				{
					surface = new Plane(direction, mytarget.transform.position + direction * mytarget.PaintPlaneOffset);
					Handles.DrawWireDisc(mytarget.transform.position + direction * mytarget.PaintPlaneOffset, direction, mytarget.PaintPlaneSize);
					Handles.zTest = UnityEngine.Rendering.CompareFunction.Less;
					Handles.color = mytarget.PaintPlaneColor;
					Handles.DrawSolidDisc(mytarget.transform.position + direction * mytarget.PaintPlaneOffset, direction, mytarget.PaintPlaneSize);
				}
				

				float enter = 0;
				if (surface.Raycast(ray, out enter))
				{
					mytarget.PaintPosition = ray.GetPoint(enter);
					hastarget = true;
					mytarget.PaintNormal = surface.normal;				
				}
			}
			else
			{
				if (Physics.Raycast(ray, out hit, 2000))
				{
					mytarget.PaintPosition = hit.point;
					hastarget = true;
					mytarget.PaintNormal = hit.normal;			
				}
			}




			if (!hastarget) return;




			Handles.color = new Color32(0, 100, 100, 100);
			
			Handles.color = new Color32(0, 150, 255, 255);

			if (e.type == EventType.KeyDown)
			{
				if (Event.current.keyCode == VoxelGeneratorSettings.CopyKey)
				{
					mytarget.CopyAtPaintPosition();	
				}
			}

			if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 1)
			{
				VoxelUndoSystem.CreateManifest();
				mytarget.PaintPosition += mytarget.PaintNormal * mytarget.NormalOffset_Right;
				mytarget.ApplyModifier(mytarget.ModeRightClick);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}

			if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 0)
			{
				VoxelUndoSystem.CreateManifest();
				mytarget.PaintPosition += mytarget.PaintNormal * mytarget.NormalOffset_Left;
				mytarget.ApplyModifier(mytarget.ModeLeftClick);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}

			if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 2)
			{
				VoxelUndoSystem.CreateManifest();
				mytarget.PaintPosition += mytarget.PaintNormal * mytarget.NormalOffset_Middle;
				mytarget.ApplyModifier(mytarget.ModeMiddleClick);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}

			if ((e.type == EventType.MouseUp))
			{
				VoxelUndoSystem.FinishManifest();			
			}

			SceneView.RepaintAll();

			if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 0)
			{
				e.Use();
			}

			if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 1)
			{
				e.Use();
			}

			if ((e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 2)
			{
				e.Use();
			}
		}


		public override void OnInspectorGUI()
		{
			GUIStyle bold = new GUIStyle();
			bold.fontStyle = FontStyle.Bold;
			bold.fontSize = 14;
			bold.richText = true;

			GUIStyle normal = new GUIStyle();
			bold.fontStyle = FontStyle.Normal;
			bold.fontSize = 12;
			bold.richText = true;

			VoxelModifier_V2_Raycaster myTarget = target as VoxelModifier_V2_Raycaster;

			DrawDefaultInspector();
			VoxelGeneratorSettings.DisplaySettingsInfo("Painting Key: ", VoxelGeneratorSettings.PaintingKey.ToString(), normal);

			if (myTarget.Modifier.HasPostProcess<VM_PostProcess_CopyPaste>())
				VoxelGeneratorSettings.DisplaySettingsInfo("Copy Key: ", VoxelGeneratorSettings.CopyKey.ToString(), normal);

			Event e = Event.current;
			switch (e.type)
			{
				case EventType.KeyDown:
					{
						if (Event.current.keyCode == VoxelGeneratorSettings.PaintingKey)
						{
							ispainting = true;
						}
						break;
					}
				case EventType.KeyUp:
					{
						if (Event.current.keyCode == VoxelGeneratorSettings.PaintingKey)
						{
							ispainting = false;
						}

						break;
					}
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Apply Left"))
			{
				for (int i = 0; i < targets.Length; i++)
				{
					var current = targets[i] as VoxelModifier_V2_Raycaster;
					if (current)
					{
						current.ApplyModifierAtTransformPoint(myTarget.ModeLeftClick);
					}
				}
			}
			
			if (GUILayout.Button("Apply Middle"))
			{
				for (int i = 0; i < targets.Length; i++)
				{
					var current = targets[i] as VoxelModifier_V2_Raycaster;
					if (current)
					{
						current.ApplyModifierAtTransformPoint(myTarget.ModeMiddleClick);
					}
				}
			}

			if (GUILayout.Button("Apply Right"))
			{
				for (int i = 0; i < targets.Length; i++)
				{
					var current = targets[i] as VoxelModifier_V2_Raycaster;
					if (current)
					{
						current.ApplyModifierAtTransformPoint(myTarget.ModeRightClick);
					}
				}
			}
			EditorGUILayout.EndHorizontal();


			if (GUI.changed)
			{
				EditorUtility.SetDirty(target);
				serializedObject.ApplyModifiedProperties();
			}
		}




	}
#endif



}
