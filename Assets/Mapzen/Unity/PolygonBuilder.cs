using System.Collections.Generic;
using System.Linq;
using Mapzen.VectorData;
using UnityEngine;
using System;

namespace Mapzen.Unity
{
    public class PolygonBuilder : IGeometryHandler
    {
        [Serializable]
        public enum ExtrusionType
        {
            TopOnly,
            TopAndSides,
            SidesOnly,
        }

        [Serializable]
        public struct Options
        {
            public Material Material;
            public ExtrusionType Extrusion;
            public float MinHeight;
            public float MaxHeight;
            public bool Enabled;
            public float LayerOffset;
        }

        public PolygonBuilder(MeshData outputMeshData, Options options, Matrix4x4 transform)
        {
            Matrix4x4 layerOffset = Matrix4x4.Translate(new Vector3(0.0f, options.LayerOffset, 0.0f));
            this.transform =  layerOffset * transform;
            this.outputMeshData = outputMeshData;
            this.options = options;
            this.coordinates = new List<float>();
            this.lastPoint = new Point();
            this.rings = new List<int>();
            this.pointsInRing = 0;
            this.extrusionVertices = new List<Vector3>();
            this.extrusionUVs = new List<Vector2>();
            this.extrusionIndices = new List<int>();
            this.polygonUVs = new List<Vector2>();
        }

        private Matrix4x4 transform;
        private MeshData outputMeshData;
        private Options options;

        // Values for the tesselator.
        private List<float> coordinates;
        private List<int> rings;
        private int pointsInRing;

        // Values for extrusions.
        private Point lastPoint;
        private List<Vector3> extrusionVertices;
        private List<Vector2> extrusionUVs;
        private List<Vector2> polygonUVs;
        private List<int> extrusionIndices;

        public void OnPoint(Point point)
        {
            bool buildWalls =
                options.Extrusion == ExtrusionType.TopAndSides ||
                options.Extrusion == ExtrusionType.SidesOnly;

            // For all but the first point in each ring, create a quad extending from the
            // previous point to the current point and from MinHeight to MaxHeight.
            if (buildWalls && pointsInRing > 0)
            {
                var p0 = lastPoint;
                var p1 = point;

                var indexOffset = extrusionVertices.Count;

                var v0 = new Vector3(p0.X, options.MaxHeight, p0.Y);
                var v1 = new Vector3(p1.X, options.MaxHeight, p1.Y);
                var v2 = new Vector3(p0.X, options.MinHeight, p0.Y);
                var v3 = new Vector3(p1.X, options.MinHeight, p1.Y);

                v0 = this.transform.MultiplyPoint(v0);
                v1 = this.transform.MultiplyPoint(v1);
                v2 = this.transform.MultiplyPoint(v2);
                v3 = this.transform.MultiplyPoint(v3);

                extrusionVertices.Add(v0);
                extrusionVertices.Add(v1);
                extrusionVertices.Add(v2);
                extrusionVertices.Add(v3);

                extrusionUVs.Add(new Vector2(1.0f, 1.0f));
                extrusionUVs.Add(new Vector2(0.0f, 1.0f));
                extrusionUVs.Add(new Vector2(1.0f, 0.0f));
                extrusionUVs.Add(new Vector2(0.0f, 0.0f));

                extrusionIndices.Add(indexOffset + 1);
                extrusionIndices.Add(indexOffset + 3);
                extrusionIndices.Add(indexOffset + 2);
                extrusionIndices.Add(indexOffset + 1);
                extrusionIndices.Add(indexOffset + 2);
                extrusionIndices.Add(indexOffset + 0);
            }

            lastPoint = point;

            // Add the current point to the buffer of coordinates for the tesselator.
            if (options.Extrusion != ExtrusionType.SidesOnly)
            {
                coordinates.Add(point.X);
                coordinates.Add(point.Y);
            }

            pointsInRing++;
        }

        public void AddUV(Vector2 uv)
        {
            polygonUVs.Add(uv);
        }

        public void OnBeginLineString()
        {
        }

        public void OnEndLineString()
        {
        }

        public void OnBeginLinearRing()
        {
            pointsInRing = 0;
        }

        public void OnEndLinearRing()
        {
            rings.Add(pointsInRing);
        }

        public void OnBeginPolygon()
        {
            coordinates.Clear();
            rings.Clear();
            extrusionVertices.Clear();
            extrusionUVs.Clear();
            extrusionIndices.Clear();
            polygonUVs.Clear();
        }

        public void OnEndPolygon()
        {
            // First add vertices and indices for extrusions.
            outputMeshData.AddElements(extrusionVertices, extrusionUVs, extrusionIndices, options.Material);

            if (coordinates.Count > 0)
            {
                List<List<Vector3>> polygon = new List<List<Vector3>>();
                int ringOffset = 0;

                for (int i = 0; i < rings.Count; ++i)
                {
                    polygon.Add(new List<Vector3>());
                    for (int ring = 0; ring < rings[i]; ++ring)
                    {
                        float x = coordinates[(ring + ringOffset) * 2];
                        float y = coordinates[(ring + ringOffset) * 2 + 1];

                        polygon[i].Add(new Vector3(x, 0.0f, y));
                    }

                    ringOffset += rings[i];
                }

                var flatData = EarcutLibrary.Flatten(polygon);

                // Then tesselate polygon interior and add vertices and indices.
                var indices = EarcutLibrary.Earcut(flatData.Vertices, flatData.Holes, flatData.Dim);

                indices.Reverse();

                var vertices = new List<Vector3>(coordinates.Count / 2);

                List<Vector2> uvs;

                if (polygonUVs.Count > 0)
                {
                    uvs = polygonUVs;
                }
                else
                {
                    uvs = new List<Vector2>(coordinates.Count / 2);
                    for (int i = 0; i < coordinates.Count; i += 2)
                    {
                        uvs.Add(new Vector2(coordinates[i], coordinates[i + 1]));
                    }
                }

                for (int i = 0; i < coordinates.Count; i += 2)
                {
                    var v = new Vector3(coordinates[i], options.MaxHeight, coordinates[i + 1]);
                    v = this.transform.MultiplyPoint(v);
                    vertices.Add(v);
                }

                outputMeshData.AddElements(vertices, uvs, indices, options.Material);
            }
        }
    }
}
