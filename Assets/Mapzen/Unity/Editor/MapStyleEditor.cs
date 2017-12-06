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

            GUILayout.Label("Editing options", labelBoldStyle);

            var liveUpdateProperty = serializedObject.FindProperty("liveUpdateEnabled");
            EditorGUILayout.PropertyField(liveUpdateProperty, new GUIContent { text = "Update RegionMap while editing" });

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            GUILayout.Label("Layers", labelBoldStyle);

            layerTreeView.Layers = mapStyle.Layers;
            layerTreeView.Reload();
            layerTreeView.OnGUI(GUILayoutUtility.GetRect(0, 500, 0, 150));

            var selectedLayers = layerTreeView.GetSelection();

            var layerArrayProperty = serializedObject.FindProperty("Layers");

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
                selectedLayers = new int[0];
                layerTreeView.SetSelection(selectedLayers);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            if (selectedLayers.Count == 1)
            {
                var index = selectedLayers[0];

                var layerProperty = layerArrayProperty.GetArrayElementAtIndex(index);

                layerProperty.isExpanded = true;

                GUILayout.Label("Layer Properties", labelBoldStyle);

                var layer = mapStyle.Layers[index];

                layer.FeatureCollection = (FeatureLayer.MapzenFeatureCollection)EditorGUILayout.EnumMaskPopup("Feature Collections", layer.FeatureCollection);

                DrawSelectedLayer(layerProperty);
            }
            else
            {
                GUILayout.Label("Select a layer to see properties", labelItalicCenteredStyle);
            }

            serializedObject.ApplyModifiedProperties();

            if (liveUpdateProperty.boolValue)
            {
                // Find the regionMap containing the style mapStyle
                var regionMaps = GameObject.FindObjectsOfType<RegionMap>();
                RegionMap map = null;
                foreach (var regionMap in regionMaps)
                {
                    var style = regionMap.Styles.Find(s => s == mapStyle);
                    if (style != null)
                    {
                        map = regionMap;
                        break;
                    }
                }

                if (map != null)
                {
                    if (GUI.changed)
                    {
                        map.LogWarnings();
                        if (map.IsValid())
                        {
                            map.DownloadTilesAsync();
                        }
                        else
                        {
                            map.LogErrors();
                        }
                    }

                    if (map.HasPendingTasks())
                    {
                        Repaint();
                        if (map.FinishedRunningTasks())
                        {
                            map.GenerateSceneGraph();
                        }
                    }
                }
            }
        }

        void DrawSelectedLayer(SerializedProperty layerProperty)
        {
            // EditorGUILayout.PropertyField(layerProperty.FindPropertyRelative("FeatureCollection"));

            EditorGUILayout.PropertyField(layerProperty.FindPropertyRelative("Matchers"), true);

            EditorGUILayout.PropertyField(layerProperty.FindPropertyRelative("Combiner"), new GUIContent { text = "Combine matchers" });

            EditorGUILayout.PropertyField(layerProperty.FindPropertyRelative("Style"), true);
        }
    }
}
