using System.Collections.Generic;

namespace Mapzen.VectorData
{
    public abstract class Tile
    {
        public TileAddress Address { get { return address; } }

        public abstract IEnumerable<FeatureCollection> FeatureCollections { get; }

        protected TileAddress address;
    }
}
