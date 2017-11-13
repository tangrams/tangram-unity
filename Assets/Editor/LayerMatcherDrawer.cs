using System;
using UnityEditor;
using UnityEngine;

namespace Mapzen.Unity.Editor
{
    [CustomPropertyDrawer(typeof(LayerMatcher))]
    public class LayerMatcherDrawer : PropertyDrawer
    {
        static string kindPropertyName = "MatcherKind";

        static string keyPropertyName = "PropertyKey";

        static string rangeMinPropertyName = "MinRange";

        static string rangeMaxPropertyName = "MaxRange";

        static string regexPatternPropertyName = "RegexPattern";

        static string valuePropertyName = "PropertyValue";

        static float lineHeight = EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.height = lineHeight;

            // Draw label.
            var kindPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var kindProperty = property.FindPropertyRelative(kindPropertyName);

            kindProperty.enumValueIndex = EditorGUI.Popup(kindPosition, kindProperty.enumValueIndex, kindProperty.enumDisplayNames);

            var kind = (LayerMatcher.Kind)kindProperty.enumValueIndex;

            EditorGUI.indentLevel++;

            var rect = new Rect(position.x, position.y + lineHeight, position.width, lineHeight);

            switch (kind)
            {
                case LayerMatcher.Kind.Property:
                    // Draw a field for a single string property name.
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative(keyPropertyName));
                    break;
                case LayerMatcher.Kind.PropertyRange:
                    // Draw a field for a string property name, a min, and a max.
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative(keyPropertyName));
                    rect.y += rect.height;
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative(rangeMinPropertyName), new GUIContent { text = ">=" });
                    rect.y += rect.height;
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative(rangeMaxPropertyName), new GUIContent { text = "<" });
                    break;
                case LayerMatcher.Kind.PropertyRegex:
                    // Draw a field for a string property name and a string regex pattern.
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative(keyPropertyName));
                    rect.y += rect.height;
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative(regexPatternPropertyName), new GUIContent { text = "With Pattern" });
                    break;
                case LayerMatcher.Kind.PropertyValue:
                    // Draw a field for a string property name and a string value.
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative(keyPropertyName));
                    rect.y += rect.height;
                    EditorGUI.PropertyField(rect, property.FindPropertyRelative(valuePropertyName), new GUIContent { text = "With Value" });
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
                case LayerMatcher.Kind.None:
                    // Nothing to do.
                    break;
                case LayerMatcher.Kind.Property:
                    rows += 1;
                    break;
                case LayerMatcher.Kind.PropertyRange:
                    rows += 3;
                    break;
                case LayerMatcher.Kind.PropertyRegex:
                    rows += 2;
                    break;
                case LayerMatcher.Kind.PropertyValue:
                    rows += 2;
                    break;
            }

            return rows * EditorGUIUtility.singleLineHeight;
        }
    }
}

