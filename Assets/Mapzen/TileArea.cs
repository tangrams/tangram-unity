using System;

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
}
