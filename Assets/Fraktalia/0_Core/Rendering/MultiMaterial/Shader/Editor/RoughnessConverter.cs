#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using Fraktalia.Core.FraktaliaAttributes;
using System.Runtime.Serialization;


namespace Fraktalia.Core.FraktaliaAttributes
{
	public class RoughnessConverter : EditorWindow
	{
		public static TextureArrayTemplate texturegenerator;
		public Vector2 scrollpos;

		Texture2D MetallicTexture;
		Color MetallicNullColor;

		Texture2D RoughnessTexture;
		Texture2D Result;

		[MenuItem("Tools/Fraktalia/Roughness Converter")]
		public static void Init()
		{

			RoughnessConverter window = (RoughnessConverter)EditorWindow.GetWindow(typeof(RoughnessConverter));
			window.name = "Roughness Converter";
			window.Show();
		}

		void OnGUI()
		{
			GUIStyle title = new GUIStyle();
			title.fontStyle = FontStyle.Bold;
			title.fontSize = 14;
			title.richText = true;

			GUIStyle bold = new GUIStyle();
			bold.fontStyle = FontStyle.Bold;
			bold.fontSize = 12;
			bold.richText = true;

			Texture2D colortex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
			colortex.SetPixel(0, 0, new Color32(240, 240, 240, 255));
			colortex.Apply();

			GUIStyle text = new GUIStyle();

			text.fontSize = 12;
			text.richText = true;
			text.wordWrap = true;
			text.margin = new RectOffset(5, 5, 5, 5);
			text.normal.background = colortex;
			text.padding = new RectOffset(5, 5, 5, 5);

			EditorStyles.textField.wordWrap = true;
	
			scrollpos = GUILayout.BeginScrollView(scrollpos, false, true, GUILayout.Width(this.position.width), GUILayout.ExpandHeight(true));

			EditorGUILayout.LabelField("This is a small tool to convert roughness and metallic texture into a metallic map for standard usage. " +
				"Use this to convert roughness texturing workflow to standard workflow. Unreal Textures usually use roughness workflow.", text);

			// Property fields for Metallic and Roughness textures
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Input Textures", title);

			MetallicTexture = (Texture2D)EditorGUILayout.ObjectField("Metallic Texture", MetallicTexture, typeof(Texture2D), false);
			MetallicNullColor = EditorGUILayout.ColorField("Metallic Null Color", MetallicNullColor);

			RoughnessTexture = (Texture2D)EditorGUILayout.ObjectField("Roughness Texture", RoughnessTexture, typeof(Texture2D), false);

			EditorGUILayout.Space();

			if (GUILayout.Button("Create MetallicGloss Map"))
			{
				Texture2D result = GenerateMetallicGlossMap(MetallicTexture, RoughnessTexture, MetallicNullColor);
				if (result != null)
				{
					SaveTextureToSameFolder(RoughnessTexture, result, "MetallicGlossMap");
					Debug.Log("MetallicGloss map created and saved successfully!");
				}
			}


			EditorGUILayout.EndScrollView();
		}

		public Texture2D GenerateMetallicGlossMap(Texture2D metallic, Texture2D roughness, Color metallicNullColor)
		{
			if (roughness == null)
			{
				Debug.LogError("Roughness texture is required.");
				return null;
			}

			// Determine texture dimensions
			int width = metallic != null ? metallic.width : roughness.width;
			int height = metallic != null ? metallic.height : roughness.height;

			if (metallic != null && (metallic.width != roughness.width || metallic.height != roughness.height))
			{
				Debug.LogError("Metallic and Roughness textures must have the same dimensions.");
				return null;
			}

			// Create the new texture
			Texture2D metallicGlossMap = new Texture2D(width, height, TextureFormat.RGBA32, false);

			// Loop through each pixel
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					// Get metallic value or use NullColor
					Color metallicColor = metallic != null ? metallic.GetPixel(x, y) : metallicNullColor;

					// Get roughness value and invert it to create glossiness
					Color roughnessColor = roughness.GetPixel(x, y);
					float glossiness = 1.0f - roughnessColor.r; // Assuming roughness uses the red channel

					// Set the metallic (red) and glossiness (alpha) channels
					metallicGlossMap.SetPixel(x, y, new Color(metallicColor.r, metallicColor.r, metallicColor.r, glossiness));
				}
			}

			// Apply the changes to the texture
			metallicGlossMap.Apply();
			return metallicGlossMap;
		}

		void SaveTextureToSameFolder(Texture2D sourceTexture, Texture2D textureToSave, string fileNameSuffix)
		{
			// Get the path of the source texture
			string path = AssetDatabase.GetAssetPath(sourceTexture);
			if (string.IsNullOrEmpty(path))
			{
				Debug.LogError("Could not find the source texture's path.");
				return;
			}

			// Get the directory of the source texture
			string directory = System.IO.Path.GetDirectoryName(path);

			// Create a new file name with the suffix
			string fileName = System.IO.Path.GetFileNameWithoutExtension(path) + "_" + fileNameSuffix + ".png";

			// Combine the directory and the new file name
			string savePath = System.IO.Path.Combine(directory, fileName);

			// Encode the texture to PNG
			byte[] bytes = textureToSave.EncodeToPNG();

			// Write the PNG to the file
			System.IO.File.WriteAllBytes(savePath, bytes);

			// Refresh the AssetDatabase to show the new file in the editor
			AssetDatabase.Refresh();

			Debug.Log($"Texture saved to {savePath}");
		}
	}
}
#endif
