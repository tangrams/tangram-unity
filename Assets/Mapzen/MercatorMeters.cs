using System;

namespace Mapzen
{
    public class MercatorMeters
    {
        public MercatorMeters(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public double x;
        public double y;

        public LngLat ToLngLat()
        {
            return Geo.Unproject(this);
        }
    }
}
