#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Fraktalia.Core.FraktaliaAttributes
{
	public class TitleDecorator : MaterialPropertyDrawer
	{
		string Title = "";
		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return 25;
		}

		public TitleDecorator(string title)
		{
			this.Title = title;
		}

		public override void OnGUI(Rect position, MaterialProperty prop, string label, MaterialEditor editor)
		{
			GUIStyle bold = new GUIStyle();
			bold.fontStyle = FontStyle.Bold; 
			bold.fontSize = 18;
			bold.richText = true;
			Color titlecolor = new Color();
			ColorUtility.TryParseHtmlString("#008000ff", out titlecolor);
			bold.normal.textColor = titlecolor;

			

			EditorGUI.LabelField(position, Title, bold);
			
		}
	}

	public class SingleLineDrawer : MaterialPropertyDrawer
	{
		string extraproperty;
		string KeyWord;

		public SingleLineDrawer()
		{
			
			extraproperty = "";
			
		}

		public SingleLineDrawer(string extraproperty)
		{
			this.extraproperty = extraproperty;	
		}

		public SingleLineDrawer(string extraproperty, string keyword)
		{
			this.extraproperty = extraproperty;
			this.KeyWord = keyword;
		}


		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			Material mat = (Material)prop.targets[0];
			if (KeyWord != null)
			{
				bool state = mat.IsKeywordEnabled(KeyWord);
				if (!state)
				{
					return 0;
				}
			}
			return base.GetPropertyHeight(prop, label, editor);
		}


		public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
		{
			Dictionary<string, int> properties = new Dictionary<string, int>();
			Dictionary<string, ShaderUtil.ShaderPropertyType> types = new Dictionary<string, ShaderUtil.ShaderPropertyType>();


			Material mat = (Material)prop.targets[0];
			if (KeyWord != null)
			{
				bool state = mat.IsKeywordEnabled(KeyWord);
				if (!state)
				{
					return;
				}
			}

			Shader shader = mat.shader;

			int count = ShaderUtil.GetPropertyCount(shader);

			for (int i = 0; i < count; i++)
			{				
				string name = ShaderUtil.GetPropertyName(shader, i);
				ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);

				types[name] = type;
				properties[name] = i;
			}


			editor.TexturePropertyMiniThumbnail(position, prop, label, "");
			
			if (properties.ContainsKey(extraproperty))
			{
				ShaderUtil.ShaderPropertyType typ = types[extraproperty];

				Rect pos;
				float value;
				switch (typ)
				{
					case ShaderUtil.ShaderPropertyType.Color:
						var color = mat.GetColor(extraproperty);

						pos = position;

						pos.xMax = position.xMax;
						pos.xMin = pos.xMax - 65;
						color = EditorGUI.ColorField(pos, color);

						mat.SetColor(extraproperty, color);

						break;
					case ShaderUtil.ShaderPropertyType.Vector:
						break;
					case ShaderUtil.ShaderPropertyType.Float:
						value = mat.GetFloat(extraproperty);
						
						pos = position;

						
						
						value = EditorGUI.FloatField(pos," ", value);

						mat.SetFloat(extraproperty, value);
						

						break;
					case ShaderUtil.ShaderPropertyType.Range:
						value = mat.GetFloat(extraproperty);
						float min = ShaderUtil.GetRangeLimits(shader, properties[extraproperty], 1);
						float max = ShaderUtil.GetRangeLimits(shader, properties[extraproperty], 2);
						pos = position;

						pos.xMax = position.xMax;
						pos.xMin = pos.xMax - pos.width * 0.52f;
						value = EditorGUI.Slider(pos, value, min, max);

						mat.SetFloat(extraproperty, value);

						

						break;
					case ShaderUtil.ShaderPropertyType.TexEnv:

			

						break;
					default:
						break;
				}



			
			}
			
			
			

			
		}
	}

	public class LayerMapsDrawer : MaterialPropertyDrawer
	{
		string _DiffuseMap;
		string _MetallicGlossMap;
		string _BumpMap;
		string _ParallaxMap;
		string _OcclusionMap;
		string _EmissionMap;
		string KeyWord;
		string Title;

		Material extract;

        public LayerMapsDrawer()
		{
			this._DiffuseMap = "";
			this._MetallicGlossMap = "";
			this._BumpMap = "";
			this._ParallaxMap = "";
			this._OcclusionMap = "";
			this._EmissionMap = "";
		}

		public LayerMapsDrawer(string title, string _DiffuseMap, string _MetallicGlossMap, string _BumpMap, string _ParallaxMap, string _OcclusionMap, string _EmissionMap)
		{
			this._DiffuseMap = _DiffuseMap;
			this._MetallicGlossMap = _MetallicGlossMap;
			this._BumpMap = _BumpMap;
			this._ParallaxMap = _ParallaxMap;
			this._OcclusionMap = _OcclusionMap;
			this._EmissionMap = _EmissionMap;
			this.Title = title;
		}

		public LayerMapsDrawer(string title, string _DiffuseMap, string _MetallicGlossMap, string _BumpMap, string _ParallaxMap, string _OcclusionMap, string _EmissionMap, string keyword)
		{
			this._DiffuseMap = _DiffuseMap;
			this._MetallicGlossMap = _MetallicGlossMap;
			this._BumpMap = _BumpMap;
			this._ParallaxMap = _ParallaxMap;
			this._OcclusionMap = _OcclusionMap;
			this._EmissionMap = _EmissionMap;
			this.KeyWord = keyword;
			this.Title = title;
		}


		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			Material mat = (Material)prop.targets[0];
			if (KeyWord != null)
			{
				bool state = mat.IsKeywordEnabled(KeyWord);
				if (!state)
				{
					return 0;
				}
			}
			return base.GetPropertyHeight(prop, label, editor);
		}


		public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
		{
			Dictionary<string, int> properties = new Dictionary<string, int>();
			Dictionary<string, ShaderUtil.ShaderPropertyType> types = new Dictionary<string, ShaderUtil.ShaderPropertyType>();


			Material mat = (Material)prop.targets[0];
			if (KeyWord != null)
			{
				bool state = mat.IsKeywordEnabled(KeyWord);
				if (!state)
				{
					return;
				}
			}

			Shader shader = mat.shader;

			int count = ShaderUtil.GetPropertyCount(shader);

			for (int i = 0; i < count; i++)
			{
				string name = ShaderUtil.GetPropertyName(shader, i);
				ShaderUtil.ShaderPropertyType type = ShaderUtil.GetPropertyType(shader, i);
				types[name] = type;
				properties[name] = i;
			}

			EditorGUILayout.BeginHorizontal();

			EditorGUILayout.LabelField(Title, GUILayout.Width(100));
			extract = (Material)EditorGUILayout.ObjectField(extract, typeof(Material), false, GUILayout.Width(100));
			if (extract)
			{
				if (GUILayout.Button("Extract", EditorStyles.miniButton))
				{
					var tex0 = extracttexture(extract, "_OcclusionMap");
					var tex1 = extracttexture(extract, "_MainTex");
					var tex2 = extracttexture(extract, "_EmissionMap");
					var tex3 = extracttexture(extract, "_ParallaxMap");
					var tex4 = extracttexture(extract, "_MetallicGlossMap");
					var tex5 = extracttexture(extract, "_BumpMap");
				
					 mat.SetTexture(_OcclusionMap, tex0);			
					 mat.SetTexture(_DiffuseMap, tex1);
					 mat.SetTexture(_EmissionMap, tex2);
					 mat.SetTexture(_ParallaxMap, tex3);
					 mat.SetTexture(_MetallicGlossMap, tex4);
					 mat.SetTexture(_BumpMap, tex5);

					EditorUtility.SetDirty(mat);
				}
			}

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if (properties.ContainsKey(_DiffuseMap))
			{
				ShaderUtil.ShaderPropertyType typ = types[_DiffuseMap];
				if(typ == ShaderUtil.ShaderPropertyType.TexEnv)
                {
					var tex = mat.GetTexture(_DiffuseMap);
					tex = TextureField("Diffuse", (Texture2D)tex);
					if (GUI.changed)
					{
						mat.SetTexture(_DiffuseMap, tex);
					}
				}	
			}

			if (properties.ContainsKey(_MetallicGlossMap))
			{			
				ShaderUtil.ShaderPropertyType typ = types[_MetallicGlossMap];
				if (typ == ShaderUtil.ShaderPropertyType.TexEnv)
				{				
					var tex = mat.GetTexture(_MetallicGlossMap);
					tex = TextureField("Metallic", (Texture2D)tex);
					if (GUI.changed)
                    {
						mat.SetTexture(_MetallicGlossMap, tex);
                    }				
				}
			}

			if (properties.ContainsKey(_BumpMap))
			{
				ShaderUtil.ShaderPropertyType typ = types[_BumpMap];
				if (typ == ShaderUtil.ShaderPropertyType.TexEnv)
				{
					var tex = mat.GetTexture(_BumpMap);
					tex = TextureField("Bump", (Texture2D)tex);
					if (GUI.changed)
					{
						mat.SetTexture(_BumpMap, tex);
					}
				}
			}

			if (properties.ContainsKey(_ParallaxMap))
			{
				ShaderUtil.ShaderPropertyType typ = types[_ParallaxMap];
				if (typ == ShaderUtil.ShaderPropertyType.TexEnv)
				{
					var tex = mat.GetTexture(_ParallaxMap);
					tex = TextureField("Height", (Texture2D)tex);
					if (GUI.changed)
					{
						mat.SetTexture(_ParallaxMap, tex);
					}
				}
			}

			if (properties.ContainsKey(_OcclusionMap))
			{
				ShaderUtil.ShaderPropertyType typ = types[_OcclusionMap];
				if (typ == ShaderUtil.ShaderPropertyType.TexEnv)
				{
					var tex = mat.GetTexture(_OcclusionMap);
					tex = TextureField("Occlusion", (Texture2D)tex);
					if (GUI.changed)
					{
						mat.SetTexture(_OcclusionMap, tex);
					}
				}
			}

			if (properties.ContainsKey(_EmissionMap))
			{
				ShaderUtil.ShaderPropertyType typ = types[_EmissionMap];
				if (typ == ShaderUtil.ShaderPropertyType.TexEnv)
				{
					var tex = mat.GetTexture(_EmissionMap);
					tex = TextureField("Emission", (Texture2D)tex);
					if (GUI.changed)
					{
						mat.SetTexture(_EmissionMap, tex);
					}
				}
			}

			EditorGUILayout.EndHorizontal();
			

		}

		private Texture2D TextureField(string name, Texture2D texture)
		{
			GUILayout.BeginVertical();
			var style = new GUIStyle(GUI.skin.label);
			style.alignment = TextAnchor.UpperLeft;
			style.fixedWidth = 50;
			GUILayout.Label(name, style);
			var result = (Texture2D)EditorGUILayout.ObjectField(texture, typeof(Texture2D), false, GUILayout.Width(50), GUILayout.Height(50));
			GUILayout.EndVertical();
			return result;
		}


		private Texture2D extracttexture(Material target, string PropertyName)
		{
			if (target == null) return null;


			if (target.HasProperty(PropertyName) && target.GetTexture(PropertyName) != null)
			{
				return (Texture2D)target.GetTexture(PropertyName);
			}
			else
			{
				return null;
			}
		}
	}


	public class MultiCompileOptionDrawer : MaterialPropertyDrawer
	{
		string KeyWord;
		string Activeonkeyword = "";
		
		public MultiCompileOptionDrawer(string keyWord)
		{
			this.KeyWord = keyWord;
		}

		public MultiCompileOptionDrawer(string keyWord, string activeonkeyword)
		{
			this.KeyWord = keyWord;
			Activeonkeyword = activeonkeyword;
		}


		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			if(Activeonkeyword != "")
			{
				Material mat = (Material)prop.targets[0];
				Shader shader = mat.shader;			
				bool state = mat.IsKeywordEnabled(Activeonkeyword);
				if (!state) return 0;
			}

			return base.GetPropertyHeight(prop, label, editor) + 20;
		}

		public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
		{


			Material mat = (Material)prop.targets[0];
			Shader shader = mat.shader;

			if (Activeonkeyword != "")
			{
				if (!mat.IsKeywordEnabled(Activeonkeyword)) return;
			}


			string labeltext = KeyWord.Replace("_", " ");
			bool state = mat.IsKeywordEnabled(KeyWord);
			GUIStyle style = new GUIStyle();
			TextAnchor anchor = style.alignment;
			anchor = TextAnchor.MiddleLeft;
			style.alignment = anchor;
			style.fontSize = 14;
			style.fontStyle = FontStyle.Bold;
			GUIContent content = new GUIContent(labeltext);

			position.yMin += 8;
			
			EditorGUI.LabelField(position, content, style);
			Rect pos = position;

			pos.xMax = position.xMax;
			pos.xMin = pos.xMax - 65;
			pos.yMin += 2;
			pos.yMax -= 2;



			var color = GUI.backgroundColor;
			if (state)
			{
				GUI.backgroundColor = Color.yellow;
			}
			if (GUI.Button(pos, "ON"))
			{
				mat.EnableKeyword(KeyWord);
			}

			GUI.backgroundColor = color;
			pos.x -= pos.width;
			if (!state)
			{
				GUI.backgroundColor = Color.yellow;
			}
			if (GUI.Button(pos, "OFF"))
			{
				mat.DisableKeyword(KeyWord);
			}

			GUI.backgroundColor = color;
		}
	}

	public class MultiCompileToggleDrawer : MaterialPropertyDrawer
	{
		string KeyWord;
		string Activeonkeyword = "";

		public MultiCompileToggleDrawer(string keyWord)
		{
			this.KeyWord = keyWord;
		}

		public MultiCompileToggleDrawer(string keyWord, string activeonkeyword)
		{
			this.KeyWord = keyWord;
			Activeonkeyword = activeonkeyword;
		}


		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			if (Activeonkeyword != "")
			{
				Material mat = (Material)prop.targets[0];
				Shader shader = mat.shader;
				bool state = mat.IsKeywordEnabled(Activeonkeyword);
				if (!state) return 0;
			}

			return base.GetPropertyHeight(prop, label, editor);
		}

		public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
		{


			Material mat = (Material)prop.targets[0];
			Shader shader = mat.shader;

			if (Activeonkeyword != "")
			{
				if (!mat.IsKeywordEnabled(Activeonkeyword)) return;
			}


			string labeltext = KeyWord.Replace("_", " ");
			bool state = mat.IsKeywordEnabled(KeyWord);
			GUIStyle style = new GUIStyle();
			TextAnchor anchor = style.alignment;
			anchor = TextAnchor.MiddleLeft;
			style.alignment = anchor;
			style.fontSize = 14;

			state = EditorGUI.Toggle(position, labeltext, state);
			GUIContent content = new GUIContent(labeltext);
				
			if (state)
			{ 
				mat.EnableKeyword(KeyWord);
			}
	
			if (!state)
			{		
				mat.DisableKeyword(KeyWord);
			}		
		}
	}


	public class KeywordDependentDrawer : MaterialPropertyDrawer
	{
		string KeyWord;
		

		public KeywordDependentDrawer(string keyWord)
		{
			this.KeyWord = keyWord;		
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			Material mat = (Material)prop.targets[0];
			Shader shader = mat.shader;

			string labeltext = KeyWord.Replace("_", " ");
			bool state = mat.IsKeywordEnabled(KeyWord);

			if (state)
			{
                if (prop.type == MaterialProperty.PropType.Texture)
                {
					return base.GetPropertyHeight(prop, label, editor) + 50;
                }
				
				return base.GetPropertyHeight(prop, label, editor);
			
			
			}
			return 0;
		}

		public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
		{


			Material mat = (Material)prop.targets[0];
			Shader shader = mat.shader;

			string labeltext = KeyWord.Replace("_", " ");
			bool state = mat.IsKeywordEnabled(KeyWord);
			if(state)
			{
				editor.DefaultShaderProperty(position, prop, label);
			}

		}
	}

}
#endif
