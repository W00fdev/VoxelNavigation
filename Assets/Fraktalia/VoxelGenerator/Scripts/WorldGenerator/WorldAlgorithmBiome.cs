using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fraktalia.Core.FraktaliaAttributes;
using Fraktalia.Utility;
using Fraktalia.Utility.NativeNoise;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;

#endif

namespace Fraktalia.VoxelGen.World
{
   

    public class WorldAlgorithmBiome : MonoBehaviour
    {
        public enum BiomeApplicationMode
        {
            Never,
            Always,
			Evaluate 
        }

        public BiomeApplicationMode ApplicationMode;
		public float EvaluateFrequency = 0.1f;
		public float EvaluateOffset;

		public float BiomeUsageValue;
		public float MinBiomeUsageValue;

        [NonSerialized]
        public WorldAlgorithmCluster[] AlgorithmCluster;

        internal void Initialize(WorldGenerator worldGenerator)
        {
            AlgorithmCluster = GetComponentsInChildren<WorldAlgorithmCluster>();
        }

        internal void EvaluateUsage(WorldGenerator worldGenerator, VoxelGenerator targetGenerator)
        {
			Vector3 hash = targetGenerator.ChunkHash;

			BiomeUsageValue = PerlinNoise_Native.Sample2D(hash.x + EvaluateOffset, hash.z + EvaluateOffset, EvaluateFrequency, 1, ref worldGenerator.Permutation);		
		}

        internal void FinalizeUsage(WorldGenerator worldGenerator, VoxelGenerator targetGenerator)
        {
			
        }

        internal bool IsActiveBiome()
        {
            if (ApplicationMode == BiomeApplicationMode.Never) return false;
            if (ApplicationMode == BiomeApplicationMode.Always) return true;

			if(ApplicationMode == BiomeApplicationMode.Evaluate)
            {
				if (BiomeUsageValue > MinBiomeUsageValue) return true;
            }

            return false;
        }

       
    }

#if UNITY_EDITOR
	[CustomEditor(typeof(WorldAlgorithmBiome), true)]
	public class WorldAlgorithmBiomeEditor : Editor
	{
		private void OnEnable()
		{
			Undo.undoRedoPerformed += UpdateWorld;
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= UpdateWorld;
			UpdateWorld();
		}

		private void UpdateWorld()
		{
			WorldAlgorithmBiome myTarget = (WorldAlgorithmBiome)target;
			var worldgenerator = myTarget.GetComponentInParent<WorldGenerator>();

			if (worldgenerator)
			{
				if (worldgenerator.referenceGenerator)
				{
					if (worldgenerator.referenceGenerator.IsInitialized)
					{
						worldgenerator.InitAlgorithms();
						worldgenerator.Generate(worldgenerator.referenceGenerator);
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

			DrawDefaultInspector();

			if (GUI.changed)
			{
				UpdateWorld();
			}
		}
	}
#endif

}
