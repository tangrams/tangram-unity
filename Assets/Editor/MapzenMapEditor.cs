using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Mapzen;
using UnityEditor;

namespace PluginEditor
{
    [CustomEditor(typeof(MapzenMap))]
    public class MapzenMapEditor : Editor
    {
        private MapzenMap mapzenMap;
        private bool showTileDataFoldout = false;
        private bool showRegionScaleRatioFoldout = false;

        void OnEnable()
        {
            this.mapzenMap = (MapzenMap)target;
        }

        public override void OnInspectorGUI()
        {
            LoadPreferences();

            TileDataFoldout();

            RegionScaleRatioFoldout();

            base.OnInspectorGUI();

            bool valid = IsValid();

            EditorConfig.SetColor(valid ?
                EditorConfig.DownloadButtonEnabledColor :
                EditorConfig.DownloadButtonDisabledColor);

            if (GUILayout.Button("Download"))
            {
                if (valid)
                {
                    LogWarnings();

                    mapzenMap.DownloadTiles();
                }
                else
                {
                    LogErrors();
                }
            }

            EditorConfig.ResetColor();

            SavePreferences();
        }

        private void SceneGroupToggle(MapzenMap mapzenMap, SceneGroup.Type type)
        {
            bool isSet = SceneGroup.Test(type, mapzenMap.GroupOptions);
            isSet = EditorGUILayout.Toggle(type.ToString(), isSet);

            mapzenMap.GroupOptions = isSet ?
                mapzenMap.GroupOptions | type :
                mapzenMap.GroupOptions & ~type;
        }

        private void TileDataFoldout()
        {
            showTileDataFoldout = EditorGUILayout.Foldout(showTileDataFoldout, "Tile data");
            if (!showTileDataFoldout)
            {
                return;
            }

            GUILayout.Label("Group by:");

            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            SceneGroupToggle(mapzenMap, SceneGroup.Type.Feature);
            SceneGroupToggle(mapzenMap, SceneGroup.Type.Filter);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            SceneGroupToggle(mapzenMap, SceneGroup.Type.Layer);
            SceneGroupToggle(mapzenMap, SceneGroup.Type.Tile);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        private void UnitScaleToggle(MapzenMap mapzenMap, RegionScaleUnits.Units unit)
        {
            bool isSet = RegionScaleUnits.Test(unit, mapzenMap.RegionScaleUnit);
            isSet = EditorGUILayout.Toggle(unit.ToString(), isSet);

            mapzenMap.RegionScaleUnit = isSet ? unit : mapzenMap.RegionScaleUnit;
        }

        private void RegionScaleRatioFoldout()
        {
            showRegionScaleRatioFoldout = EditorGUILayout.Foldout(showRegionScaleRatioFoldout, "Region Scale Ratio");
            if (!showRegionScaleRatioFoldout)
            {
                return;
            }

            mapzenMap.RegionScaleRatio = EditorGUILayout.FloatField("Scale: ", mapzenMap.RegionScaleRatio);
            GUILayout.Label("Choose world scale units: ");
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            UnitScaleToggle(mapzenMap, RegionScaleUnits.Units.Meters);
            UnitScaleToggle(mapzenMap, RegionScaleUnits.Units.KiloMeters);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            UnitScaleToggle(mapzenMap, RegionScaleUnits.Units.Miles);
            UnitScaleToggle(mapzenMap, RegionScaleUnits.Units.Feet);
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
        }

        private void LoadPreferences()
        {
            string key = typeof(MapzenMapEditor).Name;
            showTileDataFoldout = EditorPrefs.GetBool(key + ".showTileDataFoldout");
            showRegionScaleRatioFoldout = EditorPrefs.GetBool(key + ".showRegionScaleRatioFoldout");
        }

        private void SavePreferences()
        {
            string key = typeof(MapzenMapEditor).Name;
            EditorPrefs.SetBool(key + ".showTileDataFoldout", showTileDataFoldout);
            EditorPrefs.SetBool(key + ".showRegionScaleRatioFoldout", showRegionScaleRatioFoldout);
        }

        private bool IsValid()
        {
            return mapzenMap.RegionName.Length > 0 && mapzenMap.FeatureStyling.Count > 0;
        }

        private void LogWarnings()
        {
            foreach (var style in mapzenMap.FeatureStyling)
            {
                if (style == null)
                {
                    Debug.LogWarning("'Null' style provided in feature styling collection");
                    continue;
                }

                if (style.FilterStyles.Count == 0)
                {
                    Debug.LogWarning("The style " + style.name + " has no filter");
                }

                foreach (var filterStyle in style.FilterStyles)
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
            if (mapzenMap.RegionName.Length == 0)
            {
                Debug.LogError("Make sure to give a region name");
            }

            if (mapzenMap.FeatureStyling.Count == 0)
            {
                Debug.LogError("Make sure to create at least one style");
            }
        }
    }
}
