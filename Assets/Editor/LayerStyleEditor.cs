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
        [SerializeField]
        private List<BuilderEditor> builderEditors;

        [SerializeField]
        private FeatureStyle.LayerStyle layerStyle;

        [SerializeField]
        private BuilderEditor.BuilderType selectedBuilderType;

        public FeatureStyle.LayerStyle LayerStyle
        {
            get { return layerStyle; }
        }

        public LayerStyleEditor(FeatureStyle.LayerStyle layerStyle)
            : base()
        {
            this.layerStyle = layerStyle;
            this.builderEditors = new List<BuilderEditor>();

            foreach (var options in layerStyle.PolygonBuilderOptions)
            {
                this.builderEditors.Add(new BuilderEditor(options));
            }

            foreach (var options in layerStyle.PolylineBuilderOptions)
            {
                this.builderEditors.Add(new BuilderEditor(options));
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                selectedBuilderType = (BuilderEditor.BuilderType)EditorGUILayout.EnumPopup("Add builder", selectedBuilderType);

                EditorConfig.SetColor(EditorConfig.AddButtonColor);
                if (GUILayout.Button(EditorConfig.AddButtonContent, EditorConfig.SmallButtonWidth))
                {
                    var editor = new BuilderEditor(selectedBuilderType);
                    layerStyle.PolygonBuilderOptions.Add(editor.PolygonBuilderOptions);
                    layerStyle.PolylineBuilderOptions.Add(editor.PolylineBuilderOptions);
                    builderEditors.Add(editor);
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
                    builderEditors.RemoveAt(i);
                    layerStyle.PolygonBuilderOptions.Remove(editor.PolygonBuilderOptions);
                    layerStyle.PolylineBuilderOptions.Remove(editor.PolylineBuilderOptions);
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
