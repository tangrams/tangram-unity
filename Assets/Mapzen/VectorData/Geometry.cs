using System.Collections.Generic;

namespace Mapzen.VectorData
{
    public class Geometry
    {
        public List<Point> points = new List<Point>();
        public List<int> rings = new List<int>();
        public GeometryType type = GeometryType.Unknown;
    }
}
