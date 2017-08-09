using System;
using UnityEditor;
using UnityEngine;

public class EditorUtil
{
    public static void FloatField(ref string view, ref float value, GUILayoutOption options)
    {
        var str = GUILayout.TextField(view, options);

        float floatField = 0.0f;

        if (str.Length == 0)
        {
            value = 0.0f;
            view = "";
        }
        else if (float.TryParse(str, out floatField))
        {
            value = floatField;
            view = str;
        }
    }
}

