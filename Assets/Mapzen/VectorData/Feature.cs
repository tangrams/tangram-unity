using System.Collections.Generic;

namespace Mapzen.VectorData
{
    public abstract class Feature
    {
        public abstract GeometryType Type { get; }

        public abstract bool TryGetProperty(string key, out object value);

        public abstract bool HandleGeometry(IGeometryHandler handler);

        public List<Point> CopyPoints()
        {
            var copier = new GeometryCopier();
            HandleGeometry(copier);
            return copier.Points;
        }

        public List<List<Point>> CopyLineStrings()
        {
            var copier = new GeometryCopier();
            HandleGeometry(copier);
            return copier.LineStrings;
        }

        public List<List<List<Point>>> CopyPolygons()
        {
            var copier = new GeometryCopier();
            HandleGeometry(copier);
            return copier.Polygons;
        }
    }
}

