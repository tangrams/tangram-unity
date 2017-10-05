using System;
using Mapzen;
using Mapzen.VectorData;
using UnityEngine;
using System.Collections.Generic;

namespace Mapzen.Unity
{
    public class PolylineBuilder : IGeometryHandler
    {
        private PolygonBuilder polygonBuilder;
        private List<Vector2> polyline;
        private Options options;

        private float uvDistance;

        [Serializable]
        public struct Options
        {
            public Material Material;
            public bool Extrude;
            public float MinHeight;
            public float MaxHeight;
            public float Width;
            public float MiterLimit;
            public bool Enabled;
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
            uvDistance = 0.0f;
        }

        public void OnPoint(Point point)
        {
            polyline.Add(new Vector2(point.X, point.Y));
        }

        public void OnBeginLineString()
        {
            polyline.Clear();
            uvDistance = 0.0f;
        }

        private void AddUVs(float uvScaleFactor)
        {
            polygonBuilder.AddUV(new Vector2(1.0f, uvDistance));
            polygonBuilder.AddUV(new Vector2(0.0f, uvDistance));
            polygonBuilder.AddUV(new Vector2(0.0f, uvDistance + uvScaleFactor));
            polygonBuilder.AddUV(new Vector2(1.0f, uvDistance + uvScaleFactor));

            polygonBuilder.AddUV(new Vector2(1.0f, uvDistance));

            uvDistance += uvScaleFactor;
        }

        public void OnEndLineString()
        {
            if (polyline.Count < 2)
            {
                return;
            }

            float extrude = options.Width * 0.5f;
            float invWidth = 1.0f / options.Width;

            if (polyline.Count == 2)
            {
                // Trivial case, build a quad
                polygonBuilder.OnBeginPolygon();
                polygonBuilder.OnBeginLinearRing();

                var currPoint = polyline[0];
                var nextPoint = polyline[1];

                // Create a quad around the segment
                var n0 = Perp(nextPoint - currPoint) * extrude;

                polygonBuilder.OnPoint(new Point(currPoint.x - n0.x, currPoint.y - n0.y));
                polygonBuilder.OnPoint(new Point(currPoint.x + n0.x, currPoint.y + n0.y));
                polygonBuilder.OnPoint(new Point(nextPoint.x + n0.x, nextPoint.y + n0.y));
                polygonBuilder.OnPoint(new Point(nextPoint.x - n0.x, nextPoint.y - n0.y));

                // Close the polygon
                polygonBuilder.OnPoint(new Point(currPoint.x - n0.x, currPoint.y - n0.y));

                AddUVs((nextPoint - currPoint).magnitude * invWidth);

                polygonBuilder.OnEndLinearRing();
                polygonBuilder.OnEndPolygon();
            }
            else
            {
                Vector2 currPoint, nextPoint, lastPoint, lastp0 = new Vector2(), lastp1 = new Vector2();

                for (int i = 1; i < polyline.Count - 1; ++i)
                {
                    polygonBuilder.OnBeginPolygon();
                    polygonBuilder.OnBeginLinearRing();

                    currPoint = polyline[i];
                    nextPoint = polyline[i+1];
                    lastPoint = polyline[i-1];

                    var n0 = Perp(currPoint - lastPoint) * extrude;
                    var n1 = Perp(nextPoint - currPoint) * extrude;

                    // First iteration, initialize lastp1 and lastp0
                    if (i == 1)
                    {
                        lastp1 = lastPoint - n0;
                        lastp0 = lastPoint + n0;
                    }

                    Vector2 p0, p1;

                    if (Vector2.SqrMagnitude(n0 - n1) < 0.0001f)
                    {
                        // Previous and current line vectors are collinear
                        p0 = currPoint + n0;
                        p1 = currPoint - n1;
                    }
                    else
                    {
                        // Right turn:                Left turn:
                        //     n0            p0                       p1
                        //     ^            +                        +
                        //     |           /          nextPoint     /
                        //     +---------+--> n1         +---------+ currPoint
                        // lastPoint    /| currPoint     |    p0 / |
                        //          p1 + |               v      +  |
                        //               |              n1         |
                        //               +                   n0 <--+ lastPoint
                        //            nextPoint

                        // Define 2d cross product between v0(x0, y0) and v1(x1, y1) as:
                        //  v0 x v1 = v1.x * v0.y - v1.y * v0.x
                        bool isRightTurn = n1.x * n0.y - n1.y * n0.x > 0.0f;

                        Vector2 miter = Miter(lastPoint, currPoint, nextPoint,
                            isRightTurn ? n0 : -n0,
                            isRightTurn ? n1 : -n1);

                        p0 = currPoint + miter * extrude;
                        Vector2 intersection;

                        bool intersect = Intersection(lastPoint, currPoint, nextPoint,
                            isRightTurn ? -n0 : n0,
                            isRightTurn ? -n1 : n1,
                            out intersection);

                        p1 = intersect ? intersection : currPoint + n0;

                        if (!isRightTurn)
                        {
                            Util.Swap<Vector2>(ref p0, ref p1);
                        }
                    }

                    polygonBuilder.OnPoint(new Point(lastp1.x, lastp1.y));
                    polygonBuilder.OnPoint(new Point(lastp0.x, lastp0.y));
                    polygonBuilder.OnPoint(new Point(p0.x, p0.y));
                    polygonBuilder.OnPoint(new Point(p1.x, p1.y));

                    // Close the polygon
                    polygonBuilder.OnPoint(new Point(lastp1.x, lastp1.y));

                    lastp0 = p0;
                    lastp1 = p1;

                    AddUVs((currPoint - lastPoint).magnitude * invWidth);

                    polygonBuilder.OnEndLinearRing();
                    polygonBuilder.OnEndPolygon();

                    // Last point, close the polyline
                    if (i == polyline.Count - 2)
                    {
                        polygonBuilder.OnBeginPolygon();
                        polygonBuilder.OnBeginLinearRing();

                        polygonBuilder.OnPoint(new Point(lastp1.x, lastp1.y));
                        polygonBuilder.OnPoint(new Point(lastp0.x, lastp0.y));
                        polygonBuilder.OnPoint(new Point(nextPoint.x + n1.x, nextPoint.y + n1.y));
                        polygonBuilder.OnPoint(new Point(nextPoint.x - n1.x, nextPoint.y - n1.y));

                        // Close the polygon
                        polygonBuilder.OnPoint(new Point(lastp1.x, lastp1.y));

                        AddUVs((nextPoint - currPoint).magnitude * invWidth);

                        polygonBuilder.OnEndLinearRing();
                        polygonBuilder.OnEndPolygon();
                    }
                }
            }
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
        /// <param name="intersection">The 2d intersection point.</param>
        /// <returns>Whether the rays intersect.</returns>
        private static bool Intersection(Vector2 lastPoint, Vector2 currPoint, Vector2 nextPoint,
            Vector2 n0, Vector2 n1, out Vector2 intersection)
        {
            // First ray r0(t) = p0 + t.v0
            var p0 = lastPoint + n0;
            var v0 = currPoint - nextPoint;

            // Second ray r1(u) = p1 + u.v1
            var p1 = nextPoint + n1;
            var v1 = currPoint - lastPoint;

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

        Vector2 Miter(Vector2 lastPoint, Vector2 currPoint, Vector2 nextPoint, Vector2 n0, Vector2 n1) {
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

            return miter;
        }
    }
}

