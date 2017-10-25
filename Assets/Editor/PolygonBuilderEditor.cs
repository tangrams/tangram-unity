using UnityEngine;
using UnityEditor;
using Mapzen;
using System.Linq;
using Mapzen.Unity;
using System;

[Serializable]
public class PolygonBuilderEditor : EditorBase
{
    [SerializeField]
    private bool show;

    [SerializeField]
    private int selectedExtrusionType = -1;

    public PolygonBuilderEditor()
        : base()
    {
        this.show = false;
    }

    public static PolygonBuilder.Options DefaultOptions()
    {
        var defaultOptions = new PolygonBuilder.Options();

        defaultOptions.Extrusion = PolygonBuilder.ExtrusionType.TopAndSides;
        defaultOptions.Enabled = true;
        defaultOptions.TileUVs = true;
        defaultOptions.MaxHeight = 0.0f;

        return defaultOptions;
    }

    private void LoadPreferences()
    {
        show = EditorPrefs.GetBool(guid + ".show");
    }

    private void SavePreferences()
    {
        EditorPrefs.SetBool(guid + ".show", show);
    }

    public PolygonBuilder.Options OnInspectorGUI(PolygonBuilder.Options options)
    {
        LoadPreferences();

        show = EditorGUILayout.Foldout(show, "Polygon builder options");

        if (!show)
        {
            SavePreferences();
            return options;
        }

        if (selectedExtrusionType == -1)
        {
            selectedExtrusionType = (int) options.Extrusion;
        }

        options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
        var extrusionTypeList = Enum.GetValues(typeof(PolygonBuilder.ExtrusionType)).Cast<PolygonBuilder.ExtrusionType>();
        var extrusionTypeStringList = extrusionTypeList.Select(type => type.ToString());
        selectedExtrusionType = EditorGUILayout.Popup("Extrusion type: ", selectedExtrusionType, extrusionTypeStringList.ToArray());
        options.Extrusion = (PolygonBuilder.ExtrusionType) selectedExtrusionType;
        options.Enabled = EditorGUILayout.Toggle("Enabled: ", options.Enabled);
        options.TileUVs = EditorGUILayout.Toggle("Tile UVs: ", options.TileUVs);

        SavePreferences();

        return options;
    }
}
