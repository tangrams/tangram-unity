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
        private PolygonBuilder.Options options;

        public PolygonBuilder.Options Options
        {
            get { return options; }
        }

        public int OptionIndex;

        public PolygonBuilderEditor(PolygonBuilder.Options options, string name)
            : base(name)
        {
            this.options = options;
        }

        public PolygonBuilderEditor(string name)
            : base(name)
        {
            this.options = new PolygonBuilder.Options();
            this.options.Extrusion = PolygonBuilder.ExtrusionType.TopAndSides;
            this.options.Enabled = true;
            this.options.MaxHeight = 0.0f;
            this.options.Material = new Material(Shader.Find("Diffuse"));
        }

        public override void OnInspectorGUI()
        {
            options.MaxHeight = EditorGUILayout.FloatField("Max Height: ", options.MaxHeight);
            options.Extrusion = (PolygonBuilder.ExtrusionType)EditorGUILayout.EnumPopup("Extrusion type: ", options.Extrusion);
            options.Material = EditorGUILayout.ObjectField("Material:", options.Material, typeof(Material)) as Material;
            options.Enabled = EditorGUILayout.Toggle("Enabled: ", options.Enabled); 
        }
    }
}
