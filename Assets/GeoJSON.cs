using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using Point = UnityEngine.Vector3;
using Line = System.Collections.Generic.List<UnityEngine.Vector3>;
using Polygon = System.Collections.Generic.List<System.Collections.Generic.List<UnityEngine.Vector3>>;

namespace MapzenHelpers
{
	public class Properties {
		Dictionary<string, Double> numericProps;
	}

	public class Feature {
		public Feature (string name) {
			this.Lines = new List<Point> ();
			this.Polygons = new List<Line> ();
		}

		Point Point { get; set; }
		List<Point> Lines { get; set; }
		List<Line> Polygons { get; set; }
		Properties Properties { get; set; }
	}

	public class Layer {
		public Layer (string name) {
			this.Name = name;
		}

		string Name { get; }
	}

	public class GeoJSON
	{
		private JSONNode root;

		public GeoJSON (JSONNode root)
		{
			this.root = root;
		}

		public List<Layer> ExtractLayers ()
		{
			var layers = new List<Layer> ();
			foreach (JSONNode child in this.root.Children) {
				layers.Add (ExtractLayer (child));
			}

			return layers;
		}

		public Layer ExtractLayer(JSONNode layerNode) {
			// TODO: get layer name from JSONNode
			Layer layer = new Layer ("");

			foreach (JSONNode child in layerNode.Children) {

			}

			return layer;
		}
	}
}

