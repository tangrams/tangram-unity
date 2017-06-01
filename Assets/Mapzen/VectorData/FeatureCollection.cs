using System.Collections.Generic;

namespace Mapzen.VectorData
{
    public abstract class FeatureCollection
    {
        public abstract string Name { get; }

        public abstract IEnumerable<Feature> Features { get; }
    }
}
