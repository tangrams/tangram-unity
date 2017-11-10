using UnityEditor;
using UnityEngine;
using System;
using Mapzen;
using Mapzen.Unity;

namespace Mapzen.Editor
{
    [CustomEditor(typeof(FeatureStyle))]
    public class FeatureStyleEditor : UnityEditor.Editor
    {
        private FeatureStyle featureStyle;
        private StyleEditor styleEditor;

        void OnEnable()
        {
            featureStyle = (FeatureStyle)target;
            styleEditor = featureStyle.Editor as StyleEditor;

            if (styleEditor == null)
            {
                styleEditor = new StyleEditor(featureStyle);
                featureStyle.Editor = styleEditor;
            }
        }

        public override void OnInspectorGUI()
        {
            styleEditor.OnInspectorGUI();

            EditorUtility.SetDirty(featureStyle);
        }
    }
}
