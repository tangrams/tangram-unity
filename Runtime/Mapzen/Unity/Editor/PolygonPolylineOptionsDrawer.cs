using System;
using UnityEditor;
using UnityEngine;

namespace Mapzen.Unity.Editor
{
    [CustomPropertyDrawer(typeof(PolygonOptions))]
    [CustomPropertyDrawer(typeof(PolylineOptions))]
    public class PolygonPolylineOptionsDrawer : PropertyDrawer
    {
        static string enabledPropertyName = "Enabled";

        static float lineHeight = EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            var enabledProperty = property.FindPropertyRelative(enabledPropertyName);

            var toggleRect = new Rect(position.x, position.y, position.width, lineHeight);

            enabledProperty.boolValue = EditorGUI.ToggleLeft(toggleRect, label, enabledProperty.boolValue);

            if (enabledProperty.boolValue)
            {
                EditorGUI.indentLevel++;

                foreach (var item in property)
                {
                    var subproperty = item as SerializedProperty;
                    if (subproperty.name == enabledPropertyName)
                    {
                        // Skip the 'Enabled' property, since we set it using the toggle above.
                        continue;
                    }
                    position.height = lineHeight;
                    position.y += lineHeight;
                    EditorGUI.PropertyField(position, subproperty, true);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            bool enabled = property.FindPropertyRelative(enabledPropertyName).boolValue;
            // Set 'isExpanded' according to the enabled property. When 'isExpended' is true, CountInProperty() counts
            // the child properties along with this property, otherwise it just counts this property.
            property.isExpanded = enabled;
            // Reserve space for the total visible properties.
            int rows = 1;
            if (enabled)
            {
                // Subtract a row for 'Enabled', since we skip it above.
                rows = property.CountInProperty() - 1;
            }
            return rows * lineHeight;
        }
    }
}
