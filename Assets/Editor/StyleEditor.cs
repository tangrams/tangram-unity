using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.VectorData.Filters;
using System.Collections.Generic;
using System.Linq;
using System;

[Serializable]
public class StyleEditor : EditorBase
{
    [SerializeField]
    private string filterName = "";

    [SerializeField]
    private Dictionary<string, FilterStyleEditor> filterStyleEditors;

    public StyleEditor(FeatureStyle featureStyling)
        : base()
    {
        this.filterStyleEditors = new Dictionary<string, FilterStyleEditor>();
        foreach (var filterStyle in featureStyling.FilterStyles)
        {
            filterStyleEditors.Add(filterStyle.Name, new FilterStyleEditor(filterStyle));
        }
    }

    public void OnInspectorGUI(FeatureStyle style)
    {
        bool showFeatureStyle = false;
        EditorGUILayout.BeginHorizontal();
        {
            filterName = EditorGUILayout.TextField("Filter name: ", filterName);

            EditorStyle.SetColor(EditorStyle.AddButtonColor);
            if (GUILayout.Button(EditorStyle.AddButtonContent, EditorStyle.SmallButtonWidth))
            {
                var queryFilterStyleName = style.FilterStyles.Where(filterStyle => filterStyle.Name == filterName);

                if (filterName.Length == 0)
                {
                    Debug.LogError("The filter name can't be empty");
                }
                else if (queryFilterStyleName.Count() > 0)
                {
                    Debug.LogError("Filter with name " + filterName + " already exists");
                }
                else
                {
                    var filterStyle = new FeatureStyle.FilterStyle(filterName);
                    style.AddFilterStyle(filterStyle);
                    filterStyleEditors.Add(filterStyle.Name, new FilterStyleEditor(filterStyle));
                }
            }
            EditorStyle.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;

        for (int i = style.FilterStyles.Count - 1; i >= 0; i--)
        {
            var filterStyling = style.FilterStyles[i];
            var editor = filterStyleEditors[filterStyling.Name];

            var state = FoldoutEditor.OnInspectorGUI(editor.GUID.ToString(), filterStyling.Name);

            if (state.show)
            {
                editor.OnInspectorGUI(filterStyling);
            }

            if (state.markedForDeletion)
            {
                style.FilterStyles.RemoveAt(i);
                filterStyleEditors.Remove(style.Name);
            }
        }

        EditorGUI.indentLevel--;
    }
}
