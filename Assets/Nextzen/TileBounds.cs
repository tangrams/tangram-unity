using System;
using System.Collections.Generic;

namespace Nextzen
{
    [System.Serializable]
    public struct TileArea
    {
        public LngLat min;
        public LngLat max;
        public int zoom;

        public TileArea(LngLat min, LngLat max, int zoom)
        {
            this.min = min;
            this.max = max;
            this.zoom = zoom;
        }
    }

    public class TileBounds
    {
        public readonly TileAddress min;
        public readonly TileAddress max;

        private TileArea area;

        public TileBounds(TileArea area)
        {
            this.area = area;

            LngLat minLngLat = area.min;
            LngLat maxLngLat = area.max;

            if (minLngLat.latitude > maxLngLat.latitude)
            {
                Util.Swap(ref minLngLat.latitude, ref maxLngLat.latitude);
            }
            if (minLngLat.longitude > maxLngLat.longitude)
            {
                Util.Swap(ref minLngLat.longitude, ref maxLngLat.longitude);
            }

            min = minLngLat.ToTileAddress(area.zoom);
            max = maxLngLat.ToTileAddress(area.zoom);
        }

        public IEnumerable<TileAddress> TileAddressRange
        {
            get
            {
                int startX = min.x;
                int startY = max.y;

                int rangeX = Math.Abs(max.x - min.x);
                int rangeY = Math.Abs(max.y - min.y);

                for (int x = 0; x <= rangeX; ++x)
                {
                    for (int y = 0; y <= rangeY; ++y)
                    {
                        yield return new TileAddress(startX + x, startY + y, area.zoom);
                    }
                }
            }
        }
    }
}