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

		public bool TryGetProperty<N>(string propertyKey, out N node)
		{
			object propertyValue;
			if (properties.TryGetValue(propertyKey, out propertyValue))
			{
				var propertyNode = (N)propertyValue;
				if (propertyNode != null)
				{
					node = propertyNode;
					return true;
				}
			}
			node = default(N);
			return false;
		}
	}
}
