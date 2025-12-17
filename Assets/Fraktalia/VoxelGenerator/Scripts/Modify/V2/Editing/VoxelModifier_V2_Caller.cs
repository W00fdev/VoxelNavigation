using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Fraktalia.VoxelGen.Modify
{
	[ExecuteInEditMode]
	public class VoxelModifier_V2_Caller : MonoBehaviour
	{
		public bool CallOnStartEditor;
		public bool CallOnStart;
		public bool FetchChildren;
		public List<VoxelModifier_V2> Modifiers = new List<VoxelModifier_V2>();

		private float timePassed = 0f;
		private bool isWaiting = false;


		void Start()
		{
			if (Application.isPlaying)
			{
				if (CallOnStart)
				{
					ApplyModifiers();
				}
			}
		}

		public void ApplyModifiers()
		{
			if (FetchChildren)
			{
				List<VoxelModifier_V2> modifiers = new List<VoxelModifier_V2>(GetComponentsInChildren<VoxelModifier_V2>());
				Modifiers.AddRange(modifiers.FindAll((a) => Modifiers.Contains(a) == false));
			}

			for (int i = 0; i < Modifiers.Count; i++)
			{
				Modifiers[i].ApplyVoxelModifier(Modifiers[i].transform.position);
			}
		}

#if UNITY_EDITOR
		void OnEnable()
		{
			if (CallOnStartEditor && !Application.isPlaying)
			{
				isWaiting = true;
				timePassed = (float)EditorApplication.timeSinceStartup;
				EditorApplication.update += OnEditorUpdate;
			}
		}

		void OnDisable()
		{
			if (!Application.isPlaying)
			{
				EditorApplication.update -= OnEditorUpdate;
			}
		}

		void OnEditorUpdate()
		{
			if (isWaiting)
			{
				float difference = (float)EditorApplication.timeSinceStartup - timePassed;
				if (difference >= 2f)
				{
					ApplyModifiers();
					isWaiting = false;
					EditorApplication.update -= OnEditorUpdate;
				}
			}
		}
#endif
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(VoxelModifier_V2_Caller))]
	[CanEditMultipleObjects]
	public class VoxelModifier_V2_CallerEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			VoxelModifier_V2_Caller myTarget = target as VoxelModifier_V2_Caller;
			DrawDefaultInspector();
			if (GUILayout.Button("Apply"))
			{
				myTarget.ApplyModifiers();
			}
		}
	}
#endif
}
