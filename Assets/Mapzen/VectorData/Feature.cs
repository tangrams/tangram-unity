using System.Collections.Generic;

namespace Mapzen.VectorData
{
	public class Feature
	{
		public Feature()
		{
			geometry = new Geometry();
			properties = new Dictionary<string, object>();
		}

		public Geometry geometry { get; set; }
		public Dictionary<string, object> properties { get; set; }
	}
}
