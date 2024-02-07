using System.Collections.Generic;

namespace Mapzen.VectorData
{
    /// <summary>
    /// Abstract FeatureCollection base class.
    /// </summary>
    /// <remarks>
    /// FeatureCollection provides an abstract interface for accessing named groups ofvector tile features. Each vector
    /// tile format has its own implementation of FeatureCollection.
    /// </remarks>
    public abstract class FeatureCollection
    {
        /// <summary>
        /// Gets a string name identifying this collection.
        /// </summary>
        /// <value>The name.</value>
        public abstract string Name { get; }

        /// <summary>
        /// Enumerates the Features in this collection.
        /// </summary>
        /// <value>The features.</value>
        public abstract IEnumerable<Feature> Features { get; }
    }
}
