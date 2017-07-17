using System.Linq;
using System.Collections.Generic;

namespace Mapzen.VectorData
{
    public class GeometryContainer
    {
        public GeometryType Type = GeometryType.Unknown;

        public List<Point> Points;
        public List<List<Point>> LineStrings;
        public List<List<List<Point>>> Polygons;

        public GeometryContainer(Feature feature)
        {
            Type = feature.Type;
            feature.HandleGeometry(new Copier(this));
        }

        protected class Copier : IGeometryHandler
        {
            GeometryContainer container;
            List<Point> receiver;

            public Copier(GeometryContainer container)
            {
                this.container = container;
            }

            public void OnPoint(Point point)
            {
                if (receiver == null)
                {
                    container.Points = new List<Point>();
                    receiver = container.Points;
                }
                receiver.Add(point);
            }

            public void OnBeginLineString()
            {
                if (container.LineStrings == null)
                {
                    container.LineStrings = new List<List<Point>>();
                }
                container.LineStrings.Add(new List<Point>());
                receiver = container.LineStrings.Last();
            }

            public void OnEndLineString()
            {
            }

            public void OnBeginLinearRing()
            {
                var polygon = container.Polygons.Last();
                polygon.Add(new List<Point>());
                receiver = polygon.Last();
            }

            public void OnEndLinearRing()
            {
            }

            public void OnBeginPolygon()
            {
                if (container.Polygons == null)
                {
                    container.Polygons = new List<List<List<Point>>>();
                }
                container.Polygons.Add(new List<List<Point>>());
            }

            public void OnEndPolygon()
            {
            }
        }
    }
}
