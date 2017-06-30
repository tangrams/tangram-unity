using System;

namespace Mapzen.VectorData
{
    /// <summary>
    /// Interface for receiving geometry from a vector feature  as a stream of events.
    /// </summary>
    /// <remarks>
    /// Implementers of this interface should return <c>true</c> from all methods to continue receiving
    /// events or return <c>false</c> to stop processing events immediately.
    /// 
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
        bool OnPoint(Point point);

        /// <summary>
        /// Begin receiving a LineString geometry.
        /// </summary>
        bool OnBeginLineString();

        /// <summary>
        /// Finish the current LineString geometry.
        /// </summary>
        bool OnEndLineString();

        /// <summary>
        /// Begin receiving a LinearRing geometry within a Polygon.
        /// </summary>
        bool OnBeginLinearRing();

        /// <summary>
        /// Finish the current LinearRing geometry.
        /// </summary>
        bool OnEndLinearRing();

        /// <summary>
        /// Begin receiving a Polygon geometry.
        /// </summary>
        bool OnBeginPolygon();

        /// <summary>
        /// Finish the current Polygon geometry.
        /// </summary>
        bool OnEndPolygon();
    }
}
