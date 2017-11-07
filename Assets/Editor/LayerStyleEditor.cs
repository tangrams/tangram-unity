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

            foreach (var options in layerStyle.PolygonBuilderOptions)
            {
                this.builderEditors.Add(new PolygonBuilderEditor(options, "Polygon builder options"));
            }

            foreach (var options in layerStyle.PolylineBuilderOptions)
            {
                this.builderEditors.Add(new PolylineBuilderEditor(options, "Polyline builder options"));
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
                            var editor = new PolygonBuilderEditor("Polygon builder options");
                            editor.OptionIndex = layerStyle.PolygonBuilderOptions.Count;
                            layerStyle.PolygonBuilderOptions.Add(editor.Options);
                            builderEditors.Add(editor);
                        } break;
                        case BuilderType.polyline: {
                            var editor = new PolylineBuilderEditor("Polyline builder options");
                            editor.OptionIndex = layerStyle.PolylineBuilderOptions.Count;
                            layerStyle.PolylineBuilderOptions.Add(editor.Options);
                            builderEditors.Add(editor);
                        } break;
                    }
                }
                EditorConfig.ResetColor();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            for (int i = builderEditors.Count - 1; i >= 0; --i)
            {
                var editor = builderEditors[i];
                var state = FoldoutEditor.OnInspectorGUI(editor.GUID.ToString(), editor.Name);

                if (state.show)
                {
                    editor.OnInspectorGUI();
                }

                if (state.markedForDeletion)
                {
                    // Remove the editor and its associated options
                    builderEditors.RemoveAt(i);

                    if (editor is PolygonBuilderEditor)
                        layerStyle.PolygonBuilderOptions.RemoveAt((
                            (PolygonBuilderEditor)editor).OptionIndex);
                    else if (editor is PolylineBuilderEditor)
                        layerStyle.PolylineBuilderOptions.RemoveAt((
                            (PolylineBuilderEditor)editor).OptionIndex);
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
