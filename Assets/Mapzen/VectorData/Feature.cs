using System;

namespace Mapzen.VectorData
{
    /// <summary>
    /// Abstract Feature base class.
    /// </summary>
    /// <remarks>
    /// Feature provides an abstract interface for accessing vector tile features. Each vector tile format has its own
    /// implementation of Feature.
    ///
    /// A vector tile feature is defined by two components: a set of properties represented as key-value pairs, and a
    /// 2D geometry.
    /// </remarks>
    public abstract class Feature
    {
        /// <summary>
        /// Gets the type of geometry that the Feature represents.
        /// </summary>
        /// <value>The GeometryType value.</value>
        public abstract GeometryType Type { get; }

        /// <summary>
        /// Tries to get a feature property by a string key.
        /// </summary>
        /// <returns><c>true</c> if the feature has a value for the specified key, <c>false</c> otherwise.</returns>
        /// <param name="key">The key to find in the properties.</param>
        /// <param name="value">If a value is found for the given key, this object will be set to that value.
        /// The resulting object will be either a <c>bool</c>, a <c>double</c>, a <c>string</c>, or <c>null</c>.</param>
        public abstract bool TryGetProperty(string key, out object value);

        /// <summary>
        /// Provides the geometry of this feature to a handler.
        /// </summary>
        /// <returns><c>true</c>, if geometry was handled completely with no errors, <c>false</c> otherwise.</returns>
        /// <param name="handler">The handler to receive the geometry events.</param>
        public abstract bool HandleGeometry(IGeometryHandler handler);

        /// <summary>
        /// Copies the geometry of this feature into a container.
        /// </summary>
        /// <returns>The geometry copy.</returns>
        public GeometryContainer CopyGeometry()
        {
            return new GeometryContainer(this);
        }
    }
}
