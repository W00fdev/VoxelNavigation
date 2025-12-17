using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Fraktalia.VoxelGen.World;
using Fraktalia.Core.Math;
using Fraktalia.Core.FraktaliaAttributes;
using Fraktalia.VoxelGen.Modify;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;
#endif

namespace Fraktalia.VoxelGen
{
	[ExecuteInEditMode]
	public class VoxelGeneratorMultiblock : MonoBehaviour
    {
		public static Vector3 Offset = new Vector3(0.5f, 0.5f, 0.5f);

		[BeginInfo("MULTIBLOCK", order = -99999)]
		[InfoTitle("Multiblock", "This component allows you to chain multiple VoxelGenerator objects together. All VoxelGenerator objects attached to this GameObject are affected.\n\n" +
		"<b>Affected generators will automatically move into the correct position when a reference generator is defined.</b>\n", "MULTIBLOCK")]
		[InfoSection1("Setup Guide:", "In order to setup the multiblock correctly, follow these steps:\n\n" +
			"1. Add this component to an empty gameobject.\n" +
			"2. Attach a VoxelGenerator object to this GameObject as child\n" +
			"3. Assign the attached VoxelGenerator as reference to this GameObject. The assigned generator is now the reference for positioning and world generation.\n" +
			"4. Duplicate the Reference.\n" +
			"5. Move the duplicated Reference to a different position. It will automatically try to snap into the correct position.\n" +
			"6. Repeat this step until your multiblock setup is complete.", "MULTIBLOCK")]
		[InfoSection2("Important information:", "" +
			"<b>Generation:</b> The generators must all be initialized first before they are able to connect with each other. Therefore the order is always: Positioning, Initialize and Combine\n" +
			"<b>Multiblock Functions:</b>This component provides functions like Generate World or Save/Load Multiblock which are calls that affect all generators individually." +
			" This means that the effect depends on the individual setting of each generator piece.\n" +
			" World Generator and Save System utilize the Chunk Hash which can be computed automatically if <b>Update Chunk Hash</b> is checked." +
			" Also it is possible to generate world using the WorldGenerator from the reference only which can greatly affect the result!", "MULTIBLOCK")]
		[InfoSection3("Voxel Modifiers:", "Voxel Modifiers must also be adjusted for the Multiblock setup." +
			" Currently Spherecast and having the generators assinged for always modify works. I will later extend all modification tools to also support multiblock.", "MULTIBLOCK")]
		[InfoVideo("https://www.youtube.com/watch?v=UX2fbCAdxcM", false, "MULTIBLOCK")]
		[InfoText("Multiblock [EXPERIMENTAL]", "MULTIBLOCK")]

		[Tooltip("Updates the chunk hash when connecting the generators. Affects World Generator and Save System")]
		public bool UpdateChunkHash;
		[Tooltip("When generating world, . Affects World Generator and Safe System")]
		public bool UseIndividualWorlds;
		public bool UseReferenceForInitialValues;
		public VoxelGenerator Reference;

		[Header("Start Controlls")]
		public bool UseBuildSequenceOnStart;	
		public bool LoadOnStart;
		public bool WorldOnStart;
		
		public Dictionary<Vector3Int, VoxelGenerator> ConnectedChunks = new Dictionary<Vector3Int, VoxelGenerator>();

		private void Start()
        {
			if (UseBuildSequenceOnStart)
				StartCoroutine(BuildSequence());
        }

		public IEnumerator BuildSequence()
		{
			PositionGenerators();

			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			for (int i = 0; i < generators.Length; i++)
			{
				generators[i].GenerateBlock();
			}

			ConnectGenerators();

			for (int i = 0; i < generators.Length; i++)
			{				
				VoxelGenerator generator = generators[i];

				if (generator)
				{
					if (generator.savesystem && LoadOnStart && generator.savesystem.SaveDataExists)
					{
						generator.savesystem.Load();
					}
					else if (generator.worldgenerator && WorldOnStart)
					{
						WorldGenerator world = Reference.worldgenerator;
						if (UseIndividualWorlds)
						{
							world = generator.worldgenerator;
						}
						if (world)
						{
							world.Generate(generator);
							continue;
						}
					}
					else
					{
						if (UseReferenceForInitialValues)
						{
							generators[i].InitialValue = Reference.InitialValue;
							generators[i].AdditionalInitialValues = Reference.AdditionalInitialValues;
						}
						generators[i].GenerateBlock();
					}
				
					yield return new WaitForFixedUpdate();
				}				
			}
			yield return new WaitForFixedUpdate();
		}

        public void OnDrawGizmos()
        {
			if (!Reference) return;
			SnapGeneratorPosition(Reference.RootSize*0.25f, 0.1f);

			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
            for (int i = 0; i < generators.Length; i++)
            {
				VoxelUtility_V2.DrawVoxelGeneratorBoundary(generators[i], new Color32(0, 150, 0, 50), 0.95f);
            }

			VoxelUtility_V2.DrawVoxelGeneratorBoundary(Reference,new Color32(150, 150, 150, 50), 1.1f);
		}

        public void PositionGenerators()
        {	
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			Vector3 referenceOffset = Offset * Reference.RootSize;

			ConnectedChunks.Clear();
			for (int i = 0; i < generators.Length; i++)
			{
				VoxelGenerator currentGenerator = generators[i];
				Vector3 localPosition = currentGenerator.transform.localPosition + referenceOffset;

				Vector3Int chunkhash = VoxelMath.LocalPositionToChunk(localPosition, Reference.RootSize);
				ConnectedChunks[chunkhash] = currentGenerator;
				if (UpdateChunkHash)
                {
					currentGenerator.ChunkHash = chunkhash;
                }
                else
                {
					currentGenerator.ChunkHash = Vector3Int.zero;
				}

				Vector3 localposition = VoxelMath.ChunkHashToLocalPosition(chunkhash, Reference.RootSize);
				currentGenerator.transform.localPosition = localposition;
				currentGenerator.transform.localRotation = Quaternion.identity;
			}
		}

		public VoxelGenerator WorldPositionToGenerator(Vector3 worldPosition)
        {
			Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			Vector3Int chunkhash = VoxelMath.LocalPositionToChunk(localPosition, Reference.RootSize);

            for (int i = 0; i < generators.Length; i++)
            {
				if (generators[i].ChunkHash.Equals(chunkhash)) return generators[i];
            }

			return null;
		}

		public void WorldPositionToGenerators(List<VoxelGenerator> results , Vector3 worldPosition_min, Vector3 worldPosition_max)
		{
			Vector3 localPosition_min = transform.InverseTransformPoint(worldPosition_min);	
			Vector3Int chunkhash_min = VoxelMath.LocalPositionToChunk(localPosition_min, Reference.RootSize);

			Vector3 localPosition_max = transform.InverseTransformPoint(worldPosition_max);
			Vector3Int chunkhash_max = VoxelMath.LocalPositionToChunk(localPosition_max, Reference.RootSize);

            for (int x = chunkhash_min.x; x <= chunkhash_max.x; x++)
            {
				for (int y = chunkhash_min.y; y <= chunkhash_max.y; y++)
				{
					for (int z = chunkhash_min.z; z <= chunkhash_max.z; z++)
					{
						if(ConnectedChunks.TryGetValue(new Vector3Int(x, y, z), out var generator))
                        {
							results.Add(generator);
							if (results.Count > 100)
							{
								Debug.LogError("Safety System: You are trying to retrieve more than 100 chunks in this infinity voxels system.", this);
								results.Clear();
								return;
							}
						}	
					}
				}
			}	
		}

		public void LerpGeneratorPosition(float lerpSpeed)
        {
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			Vector3 referenceOffset = Offset * Reference.RootSize;
			for (int i = 0; i < generators.Length; i++)
			{
				VoxelGenerator currentGenerator = generators[i];

				Vector3 localPosition = currentGenerator.transform.localPosition + referenceOffset;
					
				Vector3Int chunkhash = VoxelMath.LocalPositionToChunk(localPosition , Reference.RootSize);
				Vector3 localposition = VoxelMath.ChunkHashToLocalPosition(chunkhash, Reference.RootSize);
				currentGenerator.transform.localPosition = Vector3.Lerp(currentGenerator.transform.localPosition, localposition, lerpSpeed);
			}
		}

		public void SnapGeneratorPosition(float minDistance, float lerpSpeed)
		{		
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			Vector3 referenceOffset = Offset * Reference.RootSize;
			for (int i = 0; i < generators.Length; i++)
			{
				VoxelGenerator currentGenerator = generators[i];
				Vector3 localPosition = currentGenerator.transform.localPosition + referenceOffset;

				Vector3Int chunkhash = VoxelMath.LocalPositionToChunk(localPosition, Reference.RootSize);
				Vector3 localposition = VoxelMath.ChunkHashToLocalPosition(chunkhash, Reference.RootSize);
				if ((currentGenerator.transform.localPosition - localposition).sqrMagnitude < minDistance*minDistance)
                {
					currentGenerator.transform.localPosition = Vector3.Lerp(currentGenerator.transform.localPosition, localposition, lerpSpeed);
				}
			}
		}

		public void DisconnectGenerators()
		{
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			
			for (int i = 0; i < generators.Length; i++)
			{
				generators[i].RemoveAllNeighbors();
				generators[i].Rebuild();
			}		
		}

		public void ConnectGenerators()
        {
			PositionGenerators();
			DisconnectGenerators();

			List<VoxelGenerator> generators = new List<VoxelGenerator>(GetComponentsInChildren<VoxelGenerator>());
			Vector3 referenceOffset = Offset * Reference.RootSize;

			
			for (int i = 0; i < generators.Count; i++)
			{
				VoxelGenerator currentGenerator = generators[i];
				Vector3 localPosition = currentGenerator.transform.localPosition + referenceOffset;

				Vector3Int chunkhash = VoxelMath.LocalPositionToChunk(localPosition, Reference.RootSize);

				int Index = 0;
				for (int z = -1; z <= 1; z++)
				{
					for (int y = -1; y <= 1; y++)
					{
						for (int x = -1; x <= 1; x++)
						{
							if (Index == 13)
							{
								Index++;
								continue;
							}
							Vector3Int hash = new Vector3Int(x, y, z);
							Vector3Int neighborhash = chunkhash + hash;

							for (int k = 0; k < generators.Count; k++)
							{
								VoxelGenerator neighbor = generators[k];
								Vector3 neighborPosition = neighbor.transform.localPosition + referenceOffset;
								Vector3Int neighborHash = VoxelMath.LocalPositionToChunk(neighborPosition, Reference.RootSize);
								if (neighborHash == neighborhash)
								{
									currentGenerator.SetNeighbor(Index, neighbor);
									break;
								}
							}

							Index++;
						}
					}
				}

				currentGenerator.Rebuild();		
			}
		}

		public void GenerateMultiBlock()
        {
			PositionGenerators();
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			for (int i = 0; i < generators.Length; i++)
			{
				if(UseReferenceForInitialValues)
                {
					generators[i].InitialValue = Reference.InitialValue;
					generators[i].AdditionalInitialValues = Reference.AdditionalInitialValues;
                }

				generators[i].GenerateBlock();			
			}
		}

		public void GenerateMultiBlock(int initialID)
		{
			PositionGenerators();
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			for (int i = 0; i < generators.Length; i++)
			{
				if (UseReferenceForInitialValues)
				{					
					generators[i].AdditionalInitialValues = Reference.AdditionalInitialValues;
				}
				generators[i].InitialValue = initialID;	
				generators[i].GenerateBlock();
			}
		}

		public void CleanUpMultiblock()
		{
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			for (int i = 0; i < generators.Length; i++)
			{
				generators[i].CleanUp();
			}
		}

		public void GenerateMultiWorld()
        {
			PositionGenerators();
			WorldGenerator world = Reference.worldgenerator;
			
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			for (int i = 0; i < generators.Length; i++)
			{
				VoxelGenerator generator = generators[i];
				if (generator)
				{
					if (UseIndividualWorlds)
					{
						world = generator.worldgenerator;
					}
					if (world) world.Generate(generator);
				}
			}
		}

		public void LoadMultiWorld()
		{
			PositionGenerators();
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			for (int i = 0; i < generators.Length; i++)
			{
				VoxelGenerator generator = generators[i];
				if (generator)
				{
					if (generator.savesystem) generator.savesystem.Load();
				}
			}
		}

		public void SaveMultiWorld()
		{
			PositionGenerators();
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			for (int i = 0; i < generators.Length; i++)
			{
				VoxelGenerator generator = generators[i];
				if (generator)
				{
					if (generator.savesystem) generator.savesystem.Save();
				}
			}
		}

        internal void RebuildHulls()
        {
			PositionGenerators();
			VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
			for (int i = 0; i < generators.Length; i++)
			{
				VoxelGenerator generator = generators[i];
				if (generator)
				{
					generator.Rebuild();
				}
			}
		}

		public bool GeneratorsInitialized
        {
            get
            {
				VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
				for (int i = 0; i < generators.Length; i++)
				{
					VoxelGenerator generator = generators[i];
					if (!generator.IsInitialized)
					{
						return false;
					}
				}
				return true;
			}
        }

        public bool IsIdle
		{
			get
			{
				VoxelGenerator[] generators = GetComponentsInChildren<VoxelGenerator>();
				for (int i = 0; i < generators.Length; i++)
				{
					VoxelGenerator generator = generators[i];
					if (!generator.IsIdle)
					{
						return false;
					}
				}
				return true;
			}
		}


		#region Editor     
#if UNITY_EDITOR
		private bool hasloaded;

		private void EditorUpdate()
		{
			if (!hasloaded)
			{
				PositionGenerators();
			}

			hasloaded = true;
		}

		private void OnEnable()
		{
			if (!Application.isPlaying)
			{				
				EditorApplication.update += EditorUpdate;
			}

		}

		private void OnDisable()
		{
			if (!Application.isPlaying)
				EditorApplication.update -= EditorUpdate;
		}
#endif
		#endregion
	}

#if UNITY_EDITOR

	[CustomEditor(typeof(VoxelGeneratorMultiblock))]
	[CanEditMultipleObjects]
	public class VoxelGeneratorMultiblockEditor : Editor
	{
        private void OnSceneGUI()
        {
			VoxelGeneratorMultiblock myTarget = (VoxelGeneratorMultiblock)target;
			if (!myTarget.Reference) return;

			VoxelGenerator[] generators = myTarget.GetComponentsInChildren<VoxelGenerator>();
	
			float rootSize = myTarget.Reference.RootSize;
			Vector3 referenceOffset = VoxelGeneratorMultiblock.Offset * rootSize;

			for (int i = 0; i < generators.Length; i++)
			{
				VoxelGenerator currentGenerator = generators[i];
				if (!currentGenerator) continue;

				Vector3 localPosition = currentGenerator.transform.localPosition + referenceOffset;
				
				Vector3Int chunkhash = VoxelMath.LocalPositionToChunk(localPosition, myTarget.Reference.RootSize);
				Vector3 localposition = VoxelMath.ChunkHashToLocalPosition(chunkhash, myTarget.Reference.RootSize);

				int Index = 0;

				for (int z = -1; z <= 1; z++)
				{
					for (int y = -1; y <= 1; y++)
					{
						for (int x = -1; x <= 1; x++)
						{
							if (Index == 13)
							{
								Index++;
								continue;
							}

							VoxelGenerator neighbor = currentGenerator.Neighbours[Index];
							if (neighbor)
							{
								Vector3Int position = MathUtilities.Convert1DTo3D(Index, 3, 3, 3);
								Vector3 offset = new Vector3(-1 + position.x, -1 + position.y, -1 + position.z) * rootSize / 4;
								
								Vector3 localpos = Vector3.one * rootSize / 2;
								Vector3 pos = currentGenerator.transform.localToWorldMatrix.MultiplyPoint(localpos + offset);
								Vector3 pos2 = currentGenerator.transform.localToWorldMatrix.MultiplyPoint(localpos + offset*3);

								Handles.color = new Color32(0, 255, 0, 255);
								Handles.DrawLine(pos, pos2);
							}

							Index++;
						}
					}
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


			EditorStyles.textField.wordWrap = true;

			VoxelGeneratorMultiblock myTarget = (VoxelGeneratorMultiblock)target;
		
			DrawDefaultInspector();

			if (!myTarget.Reference)
			{
				EditorGUILayout.LabelField("<color=red>Reference Missing!</color>", title);
				EditorGUILayout.TextArea("This multiblock setup has no reference generator assigned. Assign one!");

				return;
			}
			EditorGUILayout.LabelField("<color=green>Positioning:</color>", title);

			if (GUILayout.Button("Position Generators"))
            {
				myTarget.PositionGenerators();
            }

			EditorGUILayout.LabelField("<color=green>Generate:</color>", title);

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Generate Block ID 0", "Fills the entire volume with initialValue 0.")))
			{
			
				myTarget.GenerateMultiBlock(0);
			}

			if (GUILayout.Button(new GUIContent("Generate Block", "Fills the entire volume with initialValue.")))
			{
				myTarget.GenerateMultiBlock();
			}

			if (GUILayout.Button(new GUIContent("Generate Block ID 255", "Fills the entire volume with initialValue 255.")))
			{
				
				myTarget.GenerateMultiBlock(255);
			}
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button(new GUIContent("Generate World", "Apply World Generator to the Multiblock.")))
			{
				myTarget.GenerateMultiWorld();
			}

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(new GUIContent("Save Multiblock", "Attempts to save all data.")))
			{
				myTarget.SaveMultiWorld();
			}

			if (GUILayout.Button(new GUIContent("Load Multiblock", "Attempts load data.")))
			{
				myTarget.LoadMultiWorld();
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.LabelField("<color=green>Connect:</color>", title);
			if (GUILayout.Button("Connect Generators"))
			{
				myTarget.ConnectGenerators();
			}

			if (GUILayout.Button("Disconnect Generators"))
			{
				myTarget.DisconnectGenerators();
			}


			if (GUILayout.Button(new GUIContent("Reset", "Clears the volume.")))
			{
				myTarget.CleanUpMultiblock();

			}

			if (GUILayout.Button(new GUIContent("Rebuild Hulls", "Rebuilds all the hulls.")))
			{
				myTarget.RebuildHulls();
			}

			if (GUI.changed)
			{
				EditorUtility.SetDirty(target);
			}
		}
	}
#endif
}