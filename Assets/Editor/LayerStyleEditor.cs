using System;
using Mapzen;
using Mapzen.Unity;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace PluginEditor
{
    [SerializeField]
    public class LayerStyleEditor : EditorBase
    {
        private enum BuilderType {
            polygon,
            polyline,
        };

        [SerializeField]
        private List<IEditor> builderEditors;

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
            this.builderEditors = new List<IEditor>();

            foreach (var options in layerStyle.PolygonBuilderOptions)
            {
                this.builderEditors.Add(new PolygonBuilderEditor(options));
            }

            foreach (var options in layerStyle.PolylineBuilderOptions)
            {
                this.builderEditors.Add(new PolylineBuilderEditor(options));
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                selectedBuilderType = (BuilderType)EditorGUILayout.EnumPopup("Add builder", selectedBuilderType);

                EditorConfig.SetColor(EditorConfig.AddButtonColor);
                if (GUILayout.Button(EditorConfig.AddButtonContent, EditorConfig.SmallButtonWidth))
                {
                    switch (selectedBuilderType)
                    {
                        case BuilderType.polygon: {
                            var editor = new PolygonBuilderEditor();
                            layerStyle.PolygonBuilderOptions.Add(editor.Options);
                            builderEditors.Add(editor);
                        } break;
                        case BuilderType.polyline: {
                            var editor = new PolylineBuilderEditor();
                            layerStyle.PolylineBuilderOptions.Add(editor.Options);
                            builderEditors.Add(editor);
                        } break;
                    }
                }
                EditorConfig.ResetColor();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            foreach (var builderEditor in builderEditors)
            {
                builderEditor.OnInspectorGUI();
            }
            EditorGUI.indentLevel--;
        }
    }
}
