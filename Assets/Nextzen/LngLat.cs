using System;

namespace Nextzen
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

        public TileAddress ToTileAddress(int zoom)
        {
            double metersPerTile = Geo.EarthCircumferenceMeters / (1 << zoom);

            MercatorMeters meters = ToMercatorMeters();

            int tileX = (int)((meters.x + Geo.EarthHalfCircumferenceMeters) / metersPerTile);
            int tileY = (int)((Geo.EarthHalfCircumferenceMeters - meters.y) / metersPerTile);

            return new TileAddress(tileX, tileY, zoom);
        }
    }
}