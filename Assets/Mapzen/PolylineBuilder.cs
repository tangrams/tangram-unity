using System;
using Mapzen.VectorData;
using UnityEngine;

namespace Mapzen
{
    public class PolylineBuilder : IGeometryHandler
    {
        public struct Options
        {
            public Material Material;
            public bool Extrude;
            public float MinHeight;
            public float MaxHeight;
            public float Width;
        }

        public PolylineBuilder(MeshData outputMeshData, Options options)
        {
            this.options = options;

            var polygonOptions = new PolygonBuilder.Options();
            polygonOptions.Material = options.Material;
            polygonOptions.Extrude = options.Extrude;
            polygonOptions.MinHeight = options.MinHeight;
            polygonOptions.MaxHeight = options.MaxHeight;

            polygonBuilder = new PolygonBuilder(outputMeshData, polygonOptions);
        }

        PolygonBuilder polygonBuilder;

        Options options;

        Vector2 lastPoint;
        int pointsInLineString;

        public bool OnPoint(Point point)
        {
            var currPoint = new Vector2(point.X, point.Y);
            if (pointsInLineString > 0)
            {
                // Start a polygon for the segment from the last point in the linestring to this one.
                polygonBuilder.OnBeginPolygon();
                polygonBuilder.OnBeginLinearRing();

                // Create a quad around the segment.
                var perp = Perp(-currPoint + lastPoint) * (options.Width * 0.5f);
                polygonBuilder.OnPoint(new Point(currPoint.x - perp.x, currPoint.y - perp.y));
                polygonBuilder.OnPoint(new Point(currPoint.x + perp.x, currPoint.y + perp.y));
                polygonBuilder.OnPoint(new Point(lastPoint.x + perp.x, lastPoint.y + perp.y));
                polygonBuilder.OnPoint(new Point(lastPoint.x - perp.x, lastPoint.y - perp.y));

                // Repeat the last point to form a ring.
                polygonBuilder.OnPoint(new Point(currPoint.x - perp.x, currPoint.y - perp.y));

                // Finish the polygon.
                polygonBuilder.OnEndLinearRing();
                polygonBuilder.OnEndPolygon();
            }

            lastPoint = currPoint;
            pointsInLineString++;
            return true;
        }

        public bool OnBeginLineString()
        {
            pointsInLineString = 0;
            return true;
        }

        public bool OnEndLineString()
        {
            return true;
        }

        public bool OnBeginLinearRing()
        {
            return false;
        }

        public bool OnEndLinearRing()
        {
            return false;
        }

        public bool OnBeginPolygon()
        {
            return false;
        }

        public bool OnEndPolygon()
        {
            return false;
        }

        public static Vector2 Perp(Vector2 d)
        {
            var p = new Vector2(-d.y, d.x);
            return p.normalized;
        }
    }
}

