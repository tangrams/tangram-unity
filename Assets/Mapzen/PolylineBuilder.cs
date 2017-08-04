using System;
using Mapzen.VectorData;
using UnityEngine;
using System.Collections.Generic;

namespace Mapzen
{
    public class PolylineBuilder : IGeometryHandler
    {
        private PolygonBuilder polygonBuilder;
        private List<Vector2> polyline;
        private Options options;

        public struct Options
        {
            public Material Material;
            public bool Extrude;
            public float MinHeight;
            public float MaxHeight;
            public float Width;
            public float MiterLimit;
        }

        public PolylineBuilder(MeshData outputMeshData, Options options, Matrix4x4 transform)
        {
            this.options = options;

            var polygonOptions = new PolygonBuilder.Options();
            polygonOptions.Material = options.Material;
            polygonOptions.Extrude = options.Extrude;
            polygonOptions.MinHeight = options.MinHeight;
            polygonOptions.MaxHeight = options.MaxHeight;

            polygonBuilder = new PolygonBuilder(outputMeshData, polygonOptions, transform);
            polyline = new List<Vector2>();
        }

        public void OnPoint(Point point)
        {
            polyline.Add(new Vector2(point.X, point.Y));
        }

        public void OnBeginLineString()
        {
            polyline.Clear();
        }

        public void OnEndLineString()
        {
            if (polyline.Count < 2)
            {
                return;
            }

            Vector2 currPoint, nextPoint, lastPoint, perp;

            // Start a polygon for the segment from the last point in the linestring to this one.
            polygonBuilder.OnBeginPolygon();
            polygonBuilder.OnBeginLinearRing();

            // Trivial case, only generate a quad polygon
            if (polyline.Count == 2)
            {
                currPoint = polyline[0];
                nextPoint = polyline[1];

                // Create a quad around the segment.
                perp = Perp(nextPoint - currPoint) * (options.Width * 0.5f);
                polygonBuilder.OnPoint(new Point(currPoint.x - perp.x, currPoint.y - perp.y));
                polygonBuilder.OnPoint(new Point(currPoint.x + perp.x, currPoint.y + perp.y));
                polygonBuilder.OnPoint(new Point(nextPoint.x + perp.x, nextPoint.y + perp.y));
                polygonBuilder.OnPoint(new Point(nextPoint.x - perp.x, nextPoint.y - perp.y));
            }
            else
            {
                // The polyline has more than 2 points, generate a polygon around it
                for (int i = 1; i < polyline.Count - 1; ++i)
                {
                    currPoint = polyline[i];
                    nextPoint = polyline[i + 1];
                    lastPoint = polyline[i - 1];

                    AddPoint(i == 1, lastPoint, currPoint, nextPoint);
                }

                for (int i = polyline.Count - 2; i >= 1; --i)
                {
                    currPoint = polyline[i];
                    nextPoint = polyline[i - 1];
                    lastPoint = polyline[i + 1];

                    AddPoint(i == polyline.Count - 2, lastPoint, currPoint, nextPoint);
                }
            }

            currPoint = polyline[0];
            nextPoint = polyline[1];
            perp = Perp(nextPoint - currPoint) * (options.Width * 0.5f);

            // Repeat the last point to form a ring.
            polygonBuilder.OnPoint(new Point(currPoint.x - perp.x, currPoint.y - perp.y));

            // Finish the polygon.
            polygonBuilder.OnEndLinearRing();
            polygonBuilder.OnEndPolygon();
        }

        public void OnBeginLinearRing()
        {
        }

        public void OnEndLinearRing()
        {
        }

        public void OnBeginPolygon()
        {
        }

        public void OnEndPolygon()
        {
        }

        private static Vector2 Perp(Vector2 d)
        {
            var p = new Vector2(-d.y, d.x);
            return p.normalized;
        }

        /// <summary>
        /// Computes the intersection point between the two rays r0(t) = p0 + t.v0 and r1(u) = p1 + t.v1.
        /// </summary>
        /// <param name="p0">The origin of the first ray r0.</param>
        /// <param name="p1">The origin of the second ray r1.</param>
        /// <param name="v0">The direction of the first ray r0.</param>
        /// <param name="v1">The direction of the second ray r1.</param>
        /// <param name="intersection">The 2d intersection point.</param>
        /// <returns>Whether the rays intersect.</returns>
        private static bool Intersection(Vector2 p0, Vector2 p1, Vector2 v0, Vector2 v1, out Vector2 intersection)
        {
            intersection = new Vector2();

            var p0p1 = p1 - p0;

            var p0p1crossv0 = p0p1.x * v0.y - p0p1.y * v0.x;
            var p0p1crossv1 = p0p1.x * v1.y - p0p1.y * v1.x;

            var v1crossv0 = (v1.x * v0.y - v1.y * v0.x);

            float u = p0p1crossv1 / v1crossv0;
            float t = p0p1crossv0 / v1crossv0;

            // Collinear or parallel
            if (Math.Abs(v1crossv0) <= float.Epsilon)
            {
                return false;
            }

            if (u >= 0.0f && u <= 1.0f && t >= 0.0f && t <= 1.0f)
            {
                intersection = p0 + t * v1;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds points around the two polyline segments [lastPoint, currPoint] and [currPoint, nextPoint]
        /// on one side of the segments, builds a miter vector if the points form a 'right turn',
        /// adds an intersection point otherwise:
        ///
        /// Right turn:                Left turn:
        ///     p0        p1  p2
        ///     +         +  +
        ///     |         | /          nextPoint
        ///     +---------+--+ p3         +---------+ currPoint
        /// lastPoint     | currPoint     |    p1 / |
        ///               |               +      +  |
        ///               |              p3         |
        ///               +--+ p4             p0 +--+ lastPoint
        ///            nextPoint
        /// </summary>
        /// <param name="isFirstPoint">Whether the point is the first point in the polyline.</param>
        /// <param name="lastPoint">The last point in the polyline.</param>
        /// <param name="currPoint">The current point in the polyline.</param>
        /// <param name="nextPoint">The next point in the polyline.</param>
        private void AddPoint(bool isFirstPoint, Vector2 lastPoint, Vector2 currPoint, Vector2 nextPoint)
        {
            float extrude = options.Width * 0.5f;

            if (isFirstPoint)
            {
                // Get normal from next segment if currPoint == lastPoint.
                Vector2 perp = currPoint == lastPoint ?
                    Perp(nextPoint - currPoint) * extrude :
                    Perp(currPoint - lastPoint) * extrude;

                polygonBuilder.OnPoint(new Point(lastPoint.x - perp.x, lastPoint.y - perp.y));
                polygonBuilder.OnPoint(new Point(lastPoint.x + perp.x, lastPoint.y + perp.y));
            }

            // Early return if we have a segment with no length.
            if (currPoint == lastPoint || nextPoint == currPoint)
            {
                return;
            }

            //          ^
            //      n0  |  currPoint
            //     +----|-----+
            // lastPoint      |
            //                ---->
            //                | n1
            //                +
            //            nextPoint
            var n0 = Perp(currPoint - lastPoint) * extrude;
            var n1 = Perp(nextPoint - currPoint) * extrude;

            // Define 2d cross product between v0(x0, y0) and v1(x1, y1) as:
            //  v0 x v1 = v1.x * v0.y - v1.y * v0.x
            bool isRightTurn = n1.x * n0.y - n1.y * n0.x > 0.0f;

            // On a right turn, build the corner with a miter vector
            if (isRightTurn)
            {
                var v0 = lastPoint - currPoint;
                var v1 = nextPoint - currPoint;
                var miter = (n0 + n1).normalized;

                // theta1 and theta2 are the two angle between
                var theta = Math.Atan2(v1.y, v1.x) - Math.Atan2(v0.y, v0.x);

                if (theta < 0.0f)
                {
                    theta += 2.0 * Math.PI;
                }

                var sinHalfTheta = Math.Sin(theta * 0.5);

                // miterLength = sin(halfTheta) = length(n0) / sin(halfTheta)
                // n0 represents the normal vector scaled by the extrusion.
                // Assume that length(n0) is 1.0 for convenience and cap the
                // miter vector before applying the extrusion.
                var scale = (float)(1.0f / Math.Max(sinHalfTheta, 1e-5f));

                miter *= scale;

                // Cap the miter vector
                if (miter.magnitude > options.MiterLimit)
                {
                    // Apply the miter limit to the normalized miter vector
                    // to make its magnitude equals to 'options.MiterLimit'.
                    miter *= options.MiterLimit / miter.magnitude;
                }

                miter *= extrude;

                polygonBuilder.OnPoint(new Point(currPoint.x + miter.x, currPoint.y + miter.y));
            }
            else
            {
                // First ray r0(t) = p0 + t.v0
                var p0 = lastPoint + n0;
                var v0 = currPoint - nextPoint;

                // Second ray r1(u) = p1 + u.v1
                var p1 = nextPoint + n1;
                var v1 = currPoint - lastPoint;

                Vector2 intersection;
                if (Intersection(p0, p1, v0, v1, out intersection))
                {
                    polygonBuilder.OnPoint(new Point(intersection.x, intersection.y));
                }
                else
                {
                    // Not intersection, vectors may be colinear, simply use the normal.
                    polygonBuilder.OnPoint(new Point(currPoint.x + n0.x, currPoint.y + n0.y));
                }
            }
        }
    }
}

