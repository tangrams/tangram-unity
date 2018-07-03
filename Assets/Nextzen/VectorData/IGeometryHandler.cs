namespace Nextzen.VectorData
{
    /// <summary>
    /// Interface for receiving geometry from a vector feature  as a stream of events.
    /// </summary>
    /// <remarks>
    /// Each type of geometry produces a specific pattern of events.
    /// Point geometry:
    ///   One or more instances of OnPoint()
    /// LineString geometry:
    ///   One or more instances of (OnBeginLineString(), two or more instances of OnPoint(), OnEndLineString()).
    /// Polygon geometry:
    ///   One or more instances of (OnBeginPolygon(), one or more instances of (OnBeginLinearRing(), four or more
    ///   instances of OnPoint(), OnEndLinearRing()), OnEndPolygon()).
    /// </remarks>
    public interface IGeometryHandler
    {
        /// <summary>
        /// Receive a Point in the current geometry.
        /// </summary>
        /// <param name="point">Point.</param>
        void OnPoint(Point point);

        /// <summary>
        /// Begin receiving a LineString geometry.
        /// </summary>
        void OnBeginLineString();

        /// <summary>
        /// Finish the current LineString geometry.
        /// </summary>
        void OnEndLineString();

        /// <summary>
        /// Begin receiving a LinearRing geometry within a Polygon.
        /// </summary>
        void OnBeginLinearRing();

        /// <summary>
        /// Finish the current LinearRing geometry.
        /// </summary>
        void OnEndLinearRing();

        /// <summary>
        /// Begin receiving a Polygon geometry.
        /// </summary>
        void OnBeginPolygon();

        /// <summary>
        /// Finish the current Polygon geometry.
        /// </summary>
        void OnEndPolygon();
    }
}