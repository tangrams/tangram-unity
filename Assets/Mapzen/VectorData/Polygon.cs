using System.Collections.Generic;

namespace Mapzen.VectorData
{
    public abstract class Polygon
    {
        public IEnumerable<LineString> Rings { get; }
    }
}
