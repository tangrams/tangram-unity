using System;
using UnityEditor;
using UnityEngine;

namespace Mapzen.Editor
{
    public class FoldoutEditor
    {
        public class State
        {
            // Whether the foldout panel is shown
            public bool show;
            // Whether the foldout panel is marked for deletion
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

                EditorConfig.SetColor(EditorConfig.RemoveButtonColor);
                if (GUILayout.Button(EditorConfig.RemoveButtonContent, EditorConfig.SmallButtonWidth))
                {
                    state.markedForDeletion = true;
                }
                EditorConfig.ResetColor();
            }
            EditorGUILayout.EndHorizontal();

            SavePrefences(editorIdentifier, state.show);

            return state;
        }
    }
}
