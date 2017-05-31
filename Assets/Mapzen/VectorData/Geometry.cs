using System.Collections.Generic;

namespace Mapzen.VectorData
{
    public class Geometry
    {
        public List<Point> points = new List<Point>();
        public List<List<int>> rings = new List<List<int>>();
        public GeometryType type = GeometryType.Unknown;
    }
}
