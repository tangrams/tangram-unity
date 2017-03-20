using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

namespace MapzenHelpers
{
	public class Layer {

	}

	public class GeoJSON
	{
		private JSONNode root;

		public GeoJSON (JSONNode root)
		{
			this.root = root;
		}

		public ArrayList ExtractLayers () 
		{
			var layers = new ArrayList ();
			foreach (JSONNode child in this.root.Children) {
				layers.Add (ExtractLayer (child));
			}
			return layers;
		}

		public Layer ExtractLayer(JSONNode layerNode) {
			return new Layer();
		}
	}
}

