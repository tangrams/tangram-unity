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
        private PolylineBuilder.Options options;

        public int OptionIndex;

        public PolylineBuilder.Options Options
        {
            get { return options; }
        }

        public PolylineBuilderEditor(PolylineBuilder.Options options, string name)
            : base(name)
        {
            this.options = options;
        }

        public PolylineBuilderEditor(string name)
            : base(name)
        {
            this.options = new PolylineBuilder.Options();
            this.options.Extrusion = PolygonBuilder.ExtrusionType.TopAndSides;
            this.options.Enabled = true;
            this.options.MaxHeight = 3.0f;
            this.options.MiterLimit = 3.0f;
            this.options.Width = 15.0f;
            this.options.Material = new Material(Shader.Find("Diffuse"));
        }

        public override void OnInspectorGUI()
        {
            options.Width = EditorGUILayout.FloatField("Width: ", options.Width);
            options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
            options.Extrusion = (PolygonBuilder.ExtrusionType)EditorGUILayout.EnumPopup("Extrusion type: ", options.Extrusion);
            options.Material = EditorGUILayout.ObjectField("Material:", options.Material, typeof(Material)) as Material;
            options.Enabled = EditorGUILayout.Toggle("Enabled: ", options.Enabled);
        }
    }
}
