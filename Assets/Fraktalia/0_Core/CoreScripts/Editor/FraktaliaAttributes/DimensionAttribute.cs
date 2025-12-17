using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Fraktalia.Utility;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.SceneManagement;
using UnityEditor.PackageManager;

#endif

namespace Fraktalia.Core.FraktaliaAttributes
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
	public class DimensionAttribute : PropertyAttribute
	{
		public DimensionDefinitions dimensionDefinitions;
		public int min;
		public int max;

		// Constructor that takes min and max values
		public DimensionAttribute(int min, int max)
		{
			this.min = min;
			this.max = max;
		}
	}

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(DimensionAttribute))]
    public class DimensionAttributeDrawer : PropertyDrawer
    {
        // Set constants for button height and buttons per row
        private const float buttonHeight = 40f;
        private const int buttonsPerRow = 4;

        // Override GetPropertyHeight to dynamically calculate height
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Get the DimensionDefinitions from the MonoBehaviour
            SerializedProperty dimensionDefinitionsProp = property.serializedObject.FindProperty("_dimensionDefinitions");

            // Base height for the slider and object field
            float totalHeight = EditorGUIUtility.singleLineHeight * 2; // slider + object field

            if (dimensionDefinitionsProp != null && dimensionDefinitionsProp.objectReferenceValue != null)
            {
                DimensionDefinitions dimensionDefinitions = (DimensionDefinitions)dimensionDefinitionsProp.objectReferenceValue;
                int numDefinitions = dimensionDefinitions.dimensionDefinitions.Count;

                // Calculate rows needed for buttons (with 4 buttons per row)
                int rows = Mathf.CeilToInt(numDefinitions / (float)buttonsPerRow);

                // Add height for the buttons
                totalHeight += rows * buttonHeight;
            }

            return totalHeight;
        }

        // Override OnGUI to draw the custom inspector
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Get the DimensionDefinitions from the MonoBehaviour
            SerializedProperty dimensionDefinitionsProp = property.serializedObject.FindProperty("_dimensionDefinitions");

            EditorGUI.BeginProperty(position, label, property);
         
            // Draw the slider
            Rect sliderRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            int intValue = EditorGUI.IntSlider(sliderRect, label, property.intValue, 0, dimensionDefinitionsProp != null && dimensionDefinitionsProp.objectReferenceValue != null ? ((DimensionDefinitions)dimensionDefinitionsProp.objectReferenceValue).dimensionDefinitions.Count - 1 : 32);
            if (intValue != property.intValue)
                property.intValue = intValue;

          

            // Draw the DimensionDefinitions object field if it exists
            if (dimensionDefinitionsProp != null)
            {
                Rect objectFieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(objectFieldRect, dimensionDefinitionsProp, new GUIContent("Dimension Definitions"));
                if (property.hasMultipleDifferentValues) return;
                DimensionDefinitions dimensionDefinitions = (DimensionDefinitions)dimensionDefinitionsProp.objectReferenceValue;

                // If DimensionDefinitions is assigned, display the buttons
                if (dimensionDefinitions != null)
                {
                    // Calculate button width (divide the total width by the number of buttons per row)
                    float buttonWidth = position.width / buttonsPerRow;
                    int numDefinitions = dimensionDefinitions.dimensionDefinitions.Count;

                    // Start drawing buttons
                    for (int i = 0; i < numDefinitions; i++)
                    {
                        DimensionDefinition dimension = dimensionDefinitions.dimensionDefinitions[i];

                        // Calculate row and column for button positioning
                        int row = i / buttonsPerRow;
                        int column = i % buttonsPerRow;

                        // Calculate button position
                        Rect buttonRectBackground = new Rect(
                            position.x + column * buttonWidth,                               // X position based on column
                            position.y + (EditorGUIUtility.singleLineHeight * 2) + row * buttonHeight, // Y position based on row
                            buttonWidth,                                                    // Button width
                            buttonHeight);                                                  // Button height           


                        // Calculate button position
                        Rect buttonRect = new Rect(
                            position.x + column * buttonWidth,                               // X position based on column
                            position.y + (EditorGUIUtility.singleLineHeight * 2) + row * buttonHeight, // Y position based on row
                            buttonWidth - 10,                                               // Button width
                            buttonHeight - 10);                                             // Button height           

                        buttonRect.center = buttonRectBackground.center;

                        // Calculate button position
                        Rect buttonRectOutline = new Rect(
                            position.x + column * buttonWidth,                               // X position based on column
                            position.y + (EditorGUIUtility.singleLineHeight * 2) + row * buttonHeight, // Y position based on row
                            buttonWidth - 5,                                                // Button width
                            buttonHeight - 5);

                        buttonRectOutline.center = buttonRectBackground.center;

                        // If button is clicked, set the int value to the index of the clicked button
                        if (GUI.Button(buttonRect, GUIContent.none, GUIStyle.none)) // Invisible button to capture clicks
                        {
                            property.intValue = i;
                        }

                        // Draw the black background outline
                        GUI.DrawTexture(buttonRectOutline, MakeTex(2, 2, new Color(0, 0, 0)), ScaleMode.StretchToFill);

                        // Draw the texture to fill the button
                        if (dimension.Texture != null)
                        {
                            GUI.DrawTexture(buttonRect, dimension.Texture, ScaleMode.StretchToFill);
                        }

                        // Create a rect for the centered text
                        Rect textrect = buttonRect;
                        textrect.height /= 2;
                        textrect.center = buttonRect.center;

                        // Create a text style for the name
                        GUIStyle textstyle = new GUIStyle(GUIStyle.none);
                        textstyle.alignment = TextAnchor.MiddleCenter;
                        textstyle.fontStyle = FontStyle.Bold;
                        textstyle.normal.textColor = Color.white;
                        if (property.intValue == i)
                        {
                            // Add yellow outline when selected
                            textstyle.normal.textColor = Color.yellow;
                        }

                        // Draw the centered name on top of the texture
                        EditorGUI.LabelField(textrect, dimension.Name, textstyle);
                    }
                }
            }

            EditorGUI.EndProperty();
        }

        // Helper method to create a texture
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }
    }
#endif

}
