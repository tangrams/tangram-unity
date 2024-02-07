using System.Collections.Generic;

namespace Mapzen.VectorData
{
    /// <summary>
    /// An abstract Tile base class.
    /// </summary>
    /// <remarks>
    /// Tile provides an abstract interface for accessing vector tiles. Each vector tile format has its own
    /// implementation of Tile.
    public abstract class Tile
    {
        /// <summary>
        /// Gets the address for the location of this Tile.
        /// </summary>
        /// <value>The address.</value>
        public TileAddress Address { get { return address; } }

        /// <summary>
        /// Enumerates the collections of Features in this tile.
        /// </summary>
        /// <value>The feature collections.</value>
        public abstract IEnumerable<FeatureCollection> FeatureCollections { get; }

        protected TileAddress address;
    }
}
