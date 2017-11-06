using System;
using Mapzen;
using Mapzen.Unity;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[SerializeField]
public class LayerStyleEditor : EditorBase
{
    private enum BuilderType {
        polygon,
        polyline,
    };

    [SerializeField]
    private List<EditorBase> builderEditors;

    [SerializeField]
    private FeatureStyle.LayerStyle layerStyle;

    [SerializeField]
    private BuilderType selectedBuilderType;

    public FeatureStyle.LayerStyle LayerStyle
    {
        get { return layerStyle; }
    }

    public LayerStyleEditor(FeatureStyle.LayerStyle layerStyle)
        : base()
    {
        this.layerStyle = layerStyle;
        this.builderEditors = new List<EditorBase>();
 
        foreach (var editorData in layerStyle.PolygonBuilderEditorOptions)
        {
            var editor = new PolygonBuilderEditor();
            editorData.editorGUID = editor.GUID;
            this.builderEditors.Add(editor);
        }

        foreach (var editorData in layerStyle.PolylineBuilderEditorOptions)
        {
            var editor = new PolylineBuilderEditor();
            editorData.editorGUID = editor.GUID;
            this.builderEditors.Add(editor);
        }
    }

    public void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            selectedBuilderType = (BuilderType)EditorGUILayout.EnumPopup("Add builder", selectedBuilderType);

            EditorConfig.SetColor(EditorConfig.AddButtonColor);
            if (GUILayout.Button(EditorConfig.AddButtonContent, EditorConfig.SmallButtonWidth))
            {
                EditorBase editor = null;
                switch (selectedBuilderType) 
                {
                    case BuilderType.polygon: {
                        editor = new PolygonBuilderEditor();

                        var editorOption = new FeatureStyle.PolygonBuilderEditorOption();

                        editorOption.editorGUID = editor.GUID;
                        editorOption.option = PolygonBuilderEditor.DefaultOptions();

                        layerStyle.PolygonBuilderEditorOptions.Add(editorOption);
                    } break;
                    case BuilderType.polyline: {
                        editor = new PolylineBuilderEditor();

                        var editorOption = new FeatureStyle.PolylineBuilderEditorOption();

                        editorOption.editorGUID = editor.GUID;
                        editorOption.option = PolylineBuilderEditor.DefaultOptions();

                        layerStyle.PolylineBuilderEditorOptions.Add(editorOption);
                     } break;
                }
                
                builderEditors.Add(editor);
            }
            EditorConfig.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;
        foreach (var builderEditor in builderEditors)
        {
            if (builderEditor is PolygonBuilderEditor)
            {
                var editorOption = layerStyle.PolygonBuilderEditorOptions.Find(data => data.editorGUID == builderEditor.GUID);
                if (editorOption != null) {
                    editorOption.option = ((PolygonBuilderEditor)builderEditor).OnInspectorGUI(editorOption.option);
                }
            }
            else if (builderEditor is PolylineBuilderEditor)
            {
                var editorOption = layerStyle.PolylineBuilderEditorOptions.Find(data => data.editorGUID == builderEditor.GUID);
                if (editorOption != null) {
                    editorOption.option = ((PolylineBuilderEditor)builderEditor).OnInspectorGUI(editorOption.option);
                }
            }
        }
        EditorGUI.indentLevel--;
    }
}
