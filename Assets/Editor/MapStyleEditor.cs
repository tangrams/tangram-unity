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

        void OnEnable()
        {
            // Check whether we already had a serialized tree view state.
            if (layerTreeViewState == null)
            {
                layerTreeViewState = new TreeViewState();
            }
            layerTreeView = new FeatureLayerTreeView(layerTreeViewState);
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

            if (!deletingLayers && selectedLayers.Count == 1)
            {
                var index = selectedLayers[0];

                var layerProperty = layerArrayProperty.GetArrayElementAtIndex(index);
                EditorGUILayout.PropertyField(layerProperty, true, null);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
