using System;

namespace Mapzen
{
    public class Geo
    {
        public static readonly double EarthRadiusMeters = 6378137.0;
        public static readonly double EarthCircumferenceMeters = EarthRadiusMeters * Math.PI * 2.0;
        public static readonly double EarthHalfCircumferenceMeters = EarthRadiusMeters * Math.PI;
        public static readonly int TilePixelSize = 256;

        public static MercatorMeters Project(LngLat lngLat)
        {
            double x = lngLat.longitude * EarthHalfCircumferenceMeters / 180.0;
            double y = Math.Log(Math.Tan(0.25 * Math.PI + lngLat.latitude * Math.PI / 360.0)) * EarthRadiusMeters;
            return new MercatorMeters(x, y);
        }

        public static LngLat Unproject(MercatorMeters meters)
        {
            double longitude = meters.x * 180.0 / EarthHalfCircumferenceMeters;
            double latitude = (Math.Atan(Math.Exp(meters.y / EarthRadiusMeters)) - 0.25 * Math.PI) * 360.0 / Math.PI;
            return new LngLat(longitude, latitude);
        }
    }
}
