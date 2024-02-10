using System;

namespace Mapzen
{
    /// <summary>
    /// MercatorMeters represents a location in the coordinate system defined by the Web Mercator projection.
    /// </summary>
    /// <remarks>
    /// In MercatorMeters the vector (0, 0) corresponds to 0 degrees latitude and longitude, positive 'y' corresponds
    /// North, and positive 'x' corresponds to East. A MercatorMeters unit represents 1 actual meter of distance at the
    /// equator, but at higher or lower latitudes it represents slightly more than 1 actual meter due to the distortion
    /// in the Web Mercator projection.
    /// </remarks>
    public struct MercatorMeters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Mapzen.MercatorMeters"/> struct.
        /// </summary>
        /// <param name="x">Meters East of 0 longitude.</param>
        /// <param name="y">Meters North of 0 latitude.</param>
        public MercatorMeters(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double x;
        public double y;

        /// <summary>
        /// Get the <see cref="Mapzen.LngLat"/> that corresponds to this location. 
        /// </summary>
        /// <returns>The LngLat.</returns>
        public LngLat ToLngLat()
        {
            return Geo.Unproject(this);
        }
    }
}
