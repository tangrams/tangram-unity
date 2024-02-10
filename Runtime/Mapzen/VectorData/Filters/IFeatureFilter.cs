using System.Collections.Generic;

namespace Mapzen.VectorData.Filters
{
	public interface IFeatureFilter
	{
		IEnumerable<Feature> Filter(FeatureCollection collection);
	}
}
