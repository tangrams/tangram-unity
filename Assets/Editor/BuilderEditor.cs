using UnityEngine;
using UnityEditor;
using Mapzen;
using Mapzen.Unity;
using System.Linq;
using System;

namespace PluginEditor
{
    [Serializable]
    public class BuilderEditor : EditorBase
    {
        public enum BuilderType
        {
            Polygons,
            Polylines,
        }

        [SerializeField]
        private PolygonBuilder.Options polygonBuilderOptions;

        [SerializeField]
        private PolylineBuilder.Options polylineBuilderOptions;

        public string Name;
        public BuilderType Type;

        public PolylineBuilder.Options PolylineBuilderOptions
        {
            get { return polylineBuilderOptions; }
        }

        public PolygonBuilder.Options PolygonBuilderOptions
        {
            get { return polygonBuilderOptions; }
        }

        public BuilderEditor(PolylineBuilder.Options options)
            : base()
        {
            this.polylineBuilderOptions = options;
            this.Type = BuilderType.Polylines;
            this.Name = this.Type.ToString();
        }

        public BuilderEditor(PolygonBuilder.Options options)
            : base()
        {
            this.polygonBuilderOptions = options;
            this.Type = BuilderType.Polygons;
            this.Name = this.Type.ToString();
        }

        public BuilderEditor(BuilderType type)
            : base()
        {
            this.Type = type;
            switch (Type) {
                case BuilderType.Polylines:
                    this.polylineBuilderOptions = new PolylineBuilder.Options();
                    this.polylineBuilderOptions.Extrusion = PolygonBuilder.ExtrusionType.TopAndSides;
                    this.polylineBuilderOptions.Enabled = true;
                    this.polylineBuilderOptions.MaxHeight = 3.0f;
                    this.polylineBuilderOptions.MiterLimit = 3.0f;
                    this.polylineBuilderOptions.Width = 15.0f;
                    this.polylineBuilderOptions.Material = new Material(Shader.Find("Diffuse"));
                break;
                case BuilderType.Polygons:
                    this.polygonBuilderOptions = new PolygonBuilder.Options();
                    this.polygonBuilderOptions.Extrusion = PolygonBuilder.ExtrusionType.TopAndSides;
                    this.polygonBuilderOptions.Enabled = true;
                    this.polygonBuilderOptions.UVMode = UVMode.Tile;
                    this.polygonBuilderOptions.MaxHeight = 0.0f;
                    this.polygonBuilderOptions.Material = new Material(Shader.Find("Diffuse"));
                break;
            }
            this.Name = this.Type.ToString();
        }

        public override void OnInspectorGUI()
        {
            switch (Type) {
                case BuilderType.Polylines:
                    polylineBuilderOptions.Width = EditorGUILayout.FloatField("Width: ", polylineBuilderOptions.Width);
                    polylineBuilderOptions.MaxHeight = EditorGUILayout.FloatField("Max Height: ", polylineBuilderOptions.MaxHeight);
                    polylineBuilderOptions.Extrusion = (PolygonBuilder.ExtrusionType)EditorGUILayout.EnumPopup("Extrusion type: ", polylineBuilderOptions.Extrusion);
                    polylineBuilderOptions.Material = EditorGUILayout.ObjectField("Material:", polylineBuilderOptions.Material, typeof(Material)) as Material;
                    polylineBuilderOptions.Enabled = EditorGUILayout.Toggle("Enabled: ", polylineBuilderOptions.Enabled);
                break;
                case BuilderType.Polygons:
                    polygonBuilderOptions.MaxHeight = EditorGUILayout.FloatField("Max Height: ", polygonBuilderOptions.MaxHeight);
                    polygonBuilderOptions.Extrusion = (PolygonBuilder.ExtrusionType)EditorGUILayout.EnumPopup("Extrusion type: ", polygonBuilderOptions.Extrusion);
                    polygonBuilderOptions.Material = EditorGUILayout.ObjectField("Material:", polygonBuilderOptions.Material, typeof(Material)) as Material;
                    polygonBuilderOptions.UVMode = (UVMode)EditorGUILayout.EnumPopup("UV Mode:", polygonBuilderOptions.UVMode);
                    polygonBuilderOptions.Enabled = EditorGUILayout.Toggle("Enabled: ", polygonBuilderOptions.Enabled); 
                break;
            }
        }
    }
}
