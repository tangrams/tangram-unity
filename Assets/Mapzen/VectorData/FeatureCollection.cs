using System.Collections.Generic;

namespace Mapzen.VectorData
{
	public class FeatureCollection
	{
		public FeatureCollection(string name = "")
		{
			this.name = name;
			this.features = new List<Feature>();
		}

		public string name;
		public List<Feature> features;
	}
}
