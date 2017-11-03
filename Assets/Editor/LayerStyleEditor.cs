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
 
        foreach (var editorData in layerStyle.PolygonBuilderEditorDatas)
        {
            var editor = new PolygonBuilderEditor();
            editorData.editorGUID = editor.GUID;
            this.builderEditors.Add(editor);
        }

        foreach (var editorData in layerStyle.PolylineBuilderEditorDatas)
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
                        FeatureStyle.LayerStyle.PolygonBuilderEditorData editorData 
                            = new FeatureStyle.LayerStyle.PolygonBuilderEditorData();
                        editorData.editorGUID = editor.GUID;
                        editorData.option = PolygonBuilderEditor.DefaultOptions();
                        layerStyle.PolygonBuilderEditorDatas.Add(editorData);
                    } break;
                    case BuilderType.polyline: {
                        editor = new PolylineBuilderEditor();
                        FeatureStyle.LayerStyle.PolylineBuilderEditorData editorData 
                            = new FeatureStyle.LayerStyle.PolylineBuilderEditorData();
                        editorData.editorGUID = editor.GUID;
                        editorData.option = PolylineBuilderEditor.DefaultOptions();
                        layerStyle.PolylineBuilderEditorDatas.Add(editorData);
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
                var editorData = layerStyle.PolygonBuilderEditorDatas.Find(data => data.editorGUID == builderEditor.GUID);
                if (editorData != null) {
                    editorData.option = ((PolygonBuilderEditor)builderEditor).OnInspectorGUI(editorData.option);
                }
            } 
            else if (builderEditor is PolylineBuilderEditor)
            {
                var editorData = layerStyle.PolylineBuilderEditorDatas.Find(data => data.editorGUID == builderEditor.GUID);
                if (editorData != null) {
                    editorData.option = ((PolylineBuilderEditor)builderEditor).OnInspectorGUI(editorData.option);
                }
            }
        }
        EditorGUI.indentLevel--;
    }
}
