using UnityEngine;
using UnityEditor;
using Mapzen;

public class PolygonBuilderEditor
{
    private bool show = false;
    private PolygonBuilder.Options options;

    public PolygonBuilderEditor()
    {
        options = new PolygonBuilder.Options();
    }

    public PolygonBuilder.Options OnInspectorGUI()
    {
        show = EditorGUILayout.Foldout(show, "Polygon builder options");
        if (!show)
        {
            return options;
        }

        return options;
    }
}
