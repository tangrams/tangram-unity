using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Mapzen.Unity.Editor
{
    [CustomEditor(typeof(MapStyle))]
    public class MapStyleEditor : UnityEditor.Editor
    {
        [SerializeField]
        TreeViewState layerTreeViewState;

        FeatureLayerTreeView layerTreeView;

        GUIStyle labelBoldStyle;
        GUIStyle labelItalicCenteredStyle;

        void OnEnable()
        {
            // Check whether we already had a serialized tree view state.
            if (layerTreeViewState == null)
            {
                layerTreeViewState = new TreeViewState();
            }
            layerTreeView = new FeatureLayerTreeView(layerTreeViewState);

            labelBoldStyle = new GUIStyle { fontStyle = FontStyle.Bold };
            labelItalicCenteredStyle = new GUIStyle { fontStyle = FontStyle.Italic, alignment = TextAnchor.MiddleCenter };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            MapStyle mapStyle = serializedObject.targetObject as MapStyle;
            if (mapStyle == null)
            {
                // ????
                return;
            }

            GUILayout.Label("Layers", labelBoldStyle);

            layerTreeView.Layers = mapStyle.Layers;
            layerTreeView.Reload();
            layerTreeView.OnGUI(GUILayoutUtility.GetRect(0, 500, 0, 150));

            var selectedLayers = layerTreeView.GetSelection();

            var layerArrayProperty = serializedObject.FindProperty("Layers");

            var deletingLayers = false;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Layer"))
            {
                mapStyle.Layers.Add(new FeatureLayer("untitled"));
            }
            if (GUILayout.Button("Remove Selected"))
            {
                foreach (var index in selectedLayers.OrderByDescending(i => i))
                {
                    layerArrayProperty.DeleteArrayElementAtIndex(index);
                }
                deletingLayers = true;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            if (!deletingLayers && selectedLayers.Count == 1)
            {
                var index = selectedLayers[0];

                var layerProperty = layerArrayProperty.GetArrayElementAtIndex(index);

                layerProperty.isExpanded = true;

                GUILayout.Label("Layer Properties", labelBoldStyle);

                // EditorGUILayout.PropertyField(layerProperty, true);
                DrawSelectedLayer(layerProperty);
            }
            else
            {
                GUILayout.Label("Select a layer to see properties", labelItalicCenteredStyle);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawSelectedLayer(SerializedProperty layerProperty)
        {
            EditorGUILayout.PropertyField(layerProperty.FindPropertyRelative("FeatureCollection"));

            EditorGUILayout.PropertyField(layerProperty.FindPropertyRelative("Combiner"));

            EditorGUILayout.PropertyField(layerProperty.FindPropertyRelative("Matchers"), true);

            EditorGUILayout.PropertyField(layerProperty.FindPropertyRelative("Style"), true);
        }
    }
}
