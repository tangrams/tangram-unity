using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.Unity;
using System.Linq;
using System;

namespace PluginEditor
{
    [Serializable]
    public class PolylineBuilderEditor : EditorBase
    {
        [SerializeField]
        private bool show;

        [SerializeField]
        private PolylineBuilder.Options options;

        public PolylineBuilder.Options Options
        {
            get { return options; }
        }

        public PolylineBuilderEditor(PolylineBuilder.Options options)
            : base()
        {
            this.show = false;
            this.options = options;
        }

        public PolylineBuilderEditor()
            : base()
        {
            this.show = false;
            this.options = new PolylineBuilder.Options();
            this.options.Extrusion = PolygonBuilder.ExtrusionType.TopAndSides;
            this.options.Enabled = true;
            this.options.MaxHeight = 3.0f;
            this.options.MiterLimit = 3.0f;
            this.options.Width = 15.0f;
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

            show = EditorGUILayout.Foldout(show, "Polyline builder options");

            if (!show)
            {
                SavePreferences();
            }

            options.Width = EditorGUILayout.FloatField("Width: ", options.Width);
            options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
            options.Extrusion = (PolygonBuilder.ExtrusionType)EditorGUILayout.EnumPopup("Extrusion type: ", options.Extrusion);
            options.Material = EditorGUILayout.ObjectField("Material:", options.Material, typeof(Material)) as Material;
            options.Enabled = EditorGUILayout.Toggle("Enabled: ", options.Enabled);

            SavePreferences();
        }
    }
}
