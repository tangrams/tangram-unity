using System.Collections.Generic;

namespace Nextzen.VectorData.Filters
{
    public interface IFeatureFilter
    {
        IEnumerable<Feature> Filter(FeatureCollection collection);
    }
}