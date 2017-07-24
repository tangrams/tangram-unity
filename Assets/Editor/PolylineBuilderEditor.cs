using UnityEngine;
using UnityEditor;
using Mapzen;

public class PolylineBuilderEditor
{
    private bool show = false;
    private PolylineBuilder.Options options;

    public PolylineBuilderEditor()
    {
        options = new PolylineBuilder.Options();
    }

    public PolylineBuilder.Options OnInspectorGUI()
    {
        show = EditorGUILayout.Foldout(show, "Polyline builder options");
        if (!show)
        {
            return options;
        }

        return options;
    }
}
