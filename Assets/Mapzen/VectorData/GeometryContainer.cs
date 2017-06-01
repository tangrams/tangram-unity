using System;
using System.Collections.Generic;

namespace Mapzen.VectorData
{
    public class GeometryContainer : IGeometryHandler
    {
        // Flat list of all points in the geometry.
        private List<Point> coordinates = new List<Point>();

        // Element i in the list indicates a LineString whose points
        // start at element i-1 and end at element i in the coordinates; 
        // the first LineString begins at 0.
        private List<int> lineStringCoordinateIndices = new List<int>();

        // Element i in the list indicates a Polygon whose rings start
        // at element i-1 and end at element i in the LineStrings;
        // the first Polygon begins at 0.
        private List<int> polygonLineStringIndices = new List<int>();

        public bool OnPoint(Point point)
        {
            throw new NotImplementedException();
        }

        public bool OnBeginLineString()
        {
            throw new NotImplementedException();
        }

        public bool OnEndLineString()
        {
            throw new NotImplementedException();
        }

        public bool OnBeginLinearRing()
        {
            throw new NotImplementedException();
        }

        public bool OnEndLinearRing()
        {
            throw new NotImplementedException();
        }

        public bool OnBeginPolygon()
        {
            throw new NotImplementedException();
        }

        public bool OnEndPolygon()
        {
            throw new NotImplementedException();
        }
    }
}

