using System;

namespace Mapzen
{
    [System.Serializable]
    public struct LngLat
    {
        public LngLat(double lng, double lat)
        {
            longitude = lng;
            latitude = lat;
        }

        public double longitude;
        public double latitude;

        public LngLat WrappedToPositive()
        {
            return new LngLat(longitude - Math.Floor(longitude / 360.0) * 360.0,
                latitude - Math.Round(latitude / 180.0) * 180.0);
        }

        public MercatorMeters ToMercatorMeters()
        {
            return Geo.Project(this);
        }
    }
}
