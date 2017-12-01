using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace Mapzen.Unity.Editor
{
    [CustomEditor(typeof(RegionMap))]
    public class RegionMapEditor : UnityEditor.Editor
    {
        private RegionMap map;

        void OnEnable()
        {
            this.map = (RegionMap)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ApiKey"));
            if (GUILayout.Button("Get an API key", EditorStyles.miniButtonRight))
            {
                Application.OpenURL("https://mapzen.com/developers");
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Area"), true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("UnitsPerMeter"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("RegionName"));

            map.GroupOptions = (SceneGroupType)EditorGUILayout.EnumMaskPopup("Grouping Options", map.GroupOptions);

            // EditorGUILayout.PropertyField(serializedObject.FindProperty("GroupOptions"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("GameObjectOptions"), true);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("Styles"), true);

            bool valid = IsValid();

            EditorConfig.SetColor(valid ?
                EditorConfig.DownloadButtonEnabledColor :
                EditorConfig.DownloadButtonDisabledColor);

            if (GUILayout.Button("Download"))
            {
                if (valid)
                {
                    LogWarnings();

                    map.DownloadTilesAsync();
                }
                else
                {
                    LogErrors();
                }
            }

            if (map.HasPendingTasks())
            {
                // Go through another OnInspectorGUI cycle
                Repaint();

                if (map.FinishedRunningTasks())
                {
                    map.GenerateSceneGraph();
                }
            }

            EditorConfig.ResetColor();

            serializedObject.ApplyModifiedProperties();

            foreach (var mapStyle in map.Styles)
            {
                if (mapStyle != null)
                {
                    mapStyle.Map = map;
                }
            }
        }

        private bool IsValid()
        {
            bool hasStyle = map.Styles.Any(style => style != null);
            return map.RegionName.Length > 0 && hasStyle;
        }

        private void LogWarnings()
        {
            foreach (var style in map.Styles)
            {
                if (style == null)
                {
                    Debug.LogWarning("'Null' style provided in feature styling collection");
                    continue;
                }

                if (style.Layers.Count == 0)
                {
                    Debug.LogWarning("The style " + style.name + " has no filter");
                }

                foreach (var filterStyle in style.Layers)
                {
                    if (filterStyle.GetFilter().CollectionNameSet.Count == 0)
                    {
                        Debug.LogWarning("The style " + style.name + " has a filter selecting no layer");
                    }
                }
            }
        }

        private void LogErrors()
        {
            if (map.RegionName.Length == 0)
            {
                Debug.LogError("Make sure to give a region name");
            }

            if (!map.Styles.Any(style => style != null))
            {
                Debug.LogError("Make sure to create at least one style");
            }
        }
    }
}
