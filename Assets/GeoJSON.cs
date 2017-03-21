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
		public Properties() {
			this.NumericProperties = new Dictionary<string, Double> ();
		}

		public Dictionary<string, Double> NumericProperties { get; }
	}

	public class Feature {
		public Feature () {
			this.Lines = new List<Point> ();
			this.Polygons = new List<Line> ();
			this.Properties = new Properties ();
		}

		public Point Point { get; set; }
		public List<Point> Lines { get; set; }
		public List<Line> Polygons { get; set; }
		public Properties Properties { get; set; }
	}

	public class Layer {
		public Layer (string name) {
			this.Name = name;
			this.Features = new List<Feature> ();
		}

		public List<Feature> Features { get; }
		public string Name { get; }
	}

	public class GeoJSON
	{
		private JSONNode root;

		public GeoJSON (string geoJSONDataString)
		{
			this.root = JSON.Parse (geoJSONDataString);
		}

		public List<Layer> ExtractLayers (List<string> layerNames)
		{
			if (this.root == null) {
				return null;
			}

			var layers = new List<Layer> ();

			foreach (string layerName in layerNames) {
				JSONNode layerNode = this.root [layerName];

				if (layerNode == null) {
					Debug.Log ("Can't find layer name in GeoJSON data " + layerName);
					continue;
				}

				Layer layer = ExtractLayer (layerNode, layerName);

				if (layer != null) {
					layers.Add (layer);
				}
			}

			return layers;
		}

		private Layer ExtractLayer(JSONNode layerNode, string layerName) {
			JSONNode features = layerNode ["features"];

			if (features == null) {
				return null;
			}

			Layer layer = new Layer (layerName);

			foreach (JSONNode feature in features.Children) {
				ExtractFeature (layer, feature);
			}

			return layer;
		}

		private void ExtractFeature(Layer layer, JSONNode featureNode) {
			Feature feature = new Feature ();
			JSONNode propertiesNode = featureNode ["properties"];

			if (featureNode != null) {
				foreach (JSONNode property in propertiesNode.Children) {
					if (property ["height"] != null) {
						feature.Properties.NumericProperties.Add ("height", property.AsDouble);
					} else if (property ["min_height"] != null) {
						feature.Properties.NumericProperties.Add ("min_height", property.AsDouble);
					}
				}

				JSONNode geometryNode = featureNode ["geometry"];

				if (geometryNode != null) {
					JSONNode coords = geometryNode ["coordinates"];
					JSONNode type = geometryNode ["type"];

					if (type != null && coords != null) {
						string typeString = type.ToString ();
						Debug.Log (typeString);

						if (typeString == "Point") {
							ExtractPoint (feature, coords);
						} else if (typeString == "MultiPoint") {
							foreach (JSONNode childCoords in coords.Children) {
								ExtractPoint (feature, childCoords);
							}
						} else if (typeString == "LineString") {
							ExtractLine (feature, coords);
						} else if (typeString == "MultiLineString") {
							foreach (JSONNode childCoords in coords.Children) {
								ExtractLine (feature, childCoords);
							}
						} else if (typeString == "Polygon") {
							ExtractPolygon (feature, coords);
						} else if (typeString == "MultiPolygon") {
							foreach (JSONNode childCoords in coords.Children) {
								ExtractPolygon (feature, childCoords);
							}
						}
					}
				}
			}

			layer.Features.Add (feature);
		}

		private void ExtractPoint(Feature feature, JSONNode pointNode) {
			// TODO
		}

		private void ExtractLine(Feature feature, JSONNode lineNode) {
			// TODO
		}

		private void ExtractPolygon(Feature feature, JSONNode polygonNode) {
			// TODO
		}
	}
}

