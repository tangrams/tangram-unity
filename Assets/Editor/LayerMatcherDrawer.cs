using System;
using UnityEditor;
using UnityEngine;

namespace Mapzen.Unity.Editor
{
    [CustomPropertyDrawer(typeof(LayerMatcher))]
    public class LayerMatcherDrawer : UnityEditor.PropertyDrawer
    {
        static string kindPropertyName = "MatcherKind";

        static float lineHeight = EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Draw label.
            var kindPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var kindProperty = property.FindPropertyRelative(kindPropertyName);

            kindProperty.enumValueIndex = EditorGUI.Popup(kindPosition, kindProperty.enumValueIndex, kindProperty.enumDisplayNames);

            var kind = (LayerMatcher.Kind)kindProperty.enumValueIndex;

            EditorGUI.indentLevel++;

            var rect = new Rect(position.x, position.y + lineHeight, position.width, lineHeight);

            switch (kind)
            {
                case LayerMatcher.Kind.AllOf:
                    // TODO
                    break;
                case LayerMatcher.Kind.AnyOf:
                    // TODO
                    break;
                case LayerMatcher.Kind.None:
                    // Nothing to do.
                    break;
                case LayerMatcher.Kind.NoneOf:
                    // TODO
                    break;
                case LayerMatcher.Kind.Property:
                    // Draw a field for a single string property name.
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative("HasProperty"));
                    break;
                case LayerMatcher.Kind.PropertyRange:
                    // TODO
                    break;
                case LayerMatcher.Kind.PropertyRegex:
                    // TODO
                    break;
                case LayerMatcher.Kind.PropertyValue:
                    // Draw a field for a string property name and a string value.
                    rect.width = 200;
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative("HasProperty"));
                    rect.x += rect.width;
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative("PropertyValue"));
                    break;
            }

            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var kindProperty = property.FindPropertyRelative(kindPropertyName);

            var kind = (LayerMatcher.Kind)kindProperty.enumValueIndex;

            int rows = 1;

            switch (kind)
            {
                case LayerMatcher.Kind.AllOf:
                    // TODO
                    break;
                case LayerMatcher.Kind.AnyOf:
                    // TODO
                    break;
                case LayerMatcher.Kind.None:
                    // Nothing to do.
                    break;
                case LayerMatcher.Kind.NoneOf:
                    // TODO
                    break;
                case LayerMatcher.Kind.Property:
                    rows += 1;
                    break;
                case LayerMatcher.Kind.PropertyRange:
                    // TODO
                    break;
                case LayerMatcher.Kind.PropertyRegex:
                    // TODO
                    break;
                case LayerMatcher.Kind.PropertyValue:
                    rows += 2;
                    break;
            }

            return rows * EditorGUIUtility.singleLineHeight;
        }
    }
}

