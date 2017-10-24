using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.Unity;
using System.Linq;
using System;

[Serializable]
public class PolylineBuilderEditor : EditorBase
{
    [SerializeField]
    private bool show;

    [SerializeField]
    private int selectedExtrusionType = -1;

    public PolylineBuilderEditor()
        : base()
    {
        this.show = false;
    }

    public static PolylineBuilder.Options DefaultOptions()
    {
        var defaultOptions = new PolylineBuilder.Options();

        defaultOptions.Extrusion = PolygonBuilder.ExtrusionType.TopAndSides;
        defaultOptions.Enabled = true;
        defaultOptions.MaxHeight = 3.0f;
        defaultOptions.MiterLimit = 3.0f;
        defaultOptions.Width = 15.0f;

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

    public PolylineBuilder.Options OnInspectorGUI(PolylineBuilder.Options options)
    {
        LoadPreferences();

        show = EditorGUILayout.Foldout(show, "Polyline builder options");

        if (!show)
        {
            SavePreferences();
            return options;
        }

        if (selectedExtrusionType == -1)
        {
            selectedExtrusionType = (int) options.Extrusion;
        }

        options.Width = EditorGUILayout.FloatField("Width: ", options.Width);
        options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
        var extrusionTypeList = Enum.GetValues(typeof(PolygonBuilder.ExtrusionType)).Cast<PolygonBuilder.ExtrusionType>();
        var extrusionTypeStringList = extrusionTypeList.Select(type => type.ToString());
        selectedExtrusionType = EditorGUILayout.Popup("Extrusion type:", selectedExtrusionType, extrusionTypeStringList.ToArray());
        options.Extrusion = (PolygonBuilder.ExtrusionType) selectedExtrusionType;
        options.Enabled = EditorGUILayout.Toggle("Enabled: ", options.Enabled);

        SavePreferences();

        return options;
    }
}
