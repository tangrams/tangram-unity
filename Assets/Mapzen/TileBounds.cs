using System;
using System.Collections.Generic;

namespace Mapzen
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

            min = TileAddress.FromLngLat(area.min, area.zoom);
            max = TileAddress.FromLngLat(area.max, area.zoom);
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
