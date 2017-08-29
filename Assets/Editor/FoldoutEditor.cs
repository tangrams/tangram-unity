using System;
using UnityEditor;
using UnityEngine;

public class FoldoutEditor
{
    public class State
    {
        public bool show;
        public bool markedForDeletion;

        public State()
        {
            this.markedForDeletion = false;
            this.show = false;
        }
    }

    public static bool LoadPreferences(string editorIdentifier)
    {
        return EditorPrefs.GetBool(editorIdentifier + ".show");
    }

    public static void SavePrefences(string editorIdentifier, bool show)
    {
        EditorPrefs.SetBool(editorIdentifier + ".show", show);
    }

    public static State OnInspectorGUI(string editorIdentifier, string foldoutName)
    {
        var state = new State();

        state.show = LoadPreferences(editorIdentifier);

        EditorGUILayout.BeginHorizontal();
        {
            state.show = EditorGUILayout.Foldout(state.show, foldoutName);

            EditorStyle.SetColor(EditorStyle.RemoveButtonColor);
            if (GUILayout.Button(EditorStyle.RemoveButtonContent, EditorStyle.SmallButtonWidth))
            {
                state.markedForDeletion = true;
            }
            EditorStyle.ResetColor();
        }
        EditorGUILayout.EndHorizontal();

        SavePrefences(editorIdentifier, state.show);

        return state;
    }
}
