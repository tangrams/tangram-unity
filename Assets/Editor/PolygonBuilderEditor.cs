using UnityEngine;
using UnityEditor;
using Mapzen;
using System.Linq;
using Mapzen.Unity;
using System;

namespace PluginEditor
{
    [Serializable]
    public class PolygonBuilderEditor : EditorBase
    {
        [SerializeField]
        private bool show;

        [SerializeField]
        private PolygonBuilder.Options options;

        public PolygonBuilder.Options Options
        {
            get { return options; }
        }

        public PolygonBuilderEditor(PolygonBuilder.Options options)
            : base()
        {
            this.show = false;
            this.options = options;
        }

        public PolygonBuilderEditor()
            : base()
        {
            this.show = false;
            this.options = new PolygonBuilder.Options();
            this.options.Extrusion = PolygonBuilder.ExtrusionType.TopAndSides;
            this.options.Enabled = true;
            this.options.MaxHeight = 0.0f;
            this.options.Material = new Material(Shader.Find("Diffuse"));
        }

        private void LoadPreferences()
        {
            show = EditorPrefs.GetBool(guid + ".show");
        }

        private void SavePreferences()
        {
            EditorPrefs.SetBool(guid + ".show", show);
        }

        public override void OnInspectorGUI()
        {
            LoadPreferences();

            show = EditorGUILayout.Foldout(show, "Polygon builder options");

            if (!show)
            {
                SavePreferences();
            }

            options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
            options.Extrusion = (PolygonBuilder.ExtrusionType)EditorGUILayout.EnumPopup("Extrusion type: ", options.Extrusion);
            options.Material = EditorGUILayout.ObjectField("Material:", options.Material, typeof(Material)) as Material;
            options.Enabled = EditorGUILayout.Toggle("Enabled: ", options.Enabled);

            SavePreferences();
        }
    }
}
