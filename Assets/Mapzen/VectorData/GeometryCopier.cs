using System.Collections.Generic;
using System.Linq;

namespace Mapzen.VectorData
{
    public class GeometryCopier : IGeometryHandler
    {
        public List<Point> Points;
        public List<List<Point>> LineStrings;
        public List<List<List<Point>>> Polygons;

        private List<Point> receptacle;

        public GeometryCopier()
        {
            Reset();
        }

        public void Reset()
        {
            Points = new List<Point>();
            LineStrings = new List<List<Point>>();
            Polygons = new List<List<List<Point>>>();

            receptacle = Points;
        }

        public bool OnPoint(Point point)
        {
            receptacle.Add(point);
            return true;
        }

        public bool OnBeginLineString()
        {
            LineStrings.Add(new List<Point>());
            receptacle = LineStrings.Last();
            return true;
        }

        public bool OnEndLineString()
        {
            return true;
        }

        public bool OnBeginLinearRing()
        {
            var polygon = Polygons.Last();
            polygon.Add(new List<Point>());
            receptacle = polygon.Last();
            return true;
        }

        public bool OnEndLinearRing()
        {
            return true;
        }

        public bool OnBeginPolygon()
        {
            Polygons.Add(new List<List<Point>>());
            return true;
        }

        public bool OnEndPolygon()
        {
            return true;
        }
    }
}

