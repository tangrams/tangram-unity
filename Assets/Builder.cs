using System;
using Mapzen.VectorData;
using LibTessDotNet;
using UnityEngine;

public class Builder
{
        public static MeshData TesselatePolygon(Geometry geometry, Color color, float height)
        {
                var meshData = new MeshData();
                var tess = new Tess();

                int pointIndex = 0;
                foreach (var ringSize in geometry.rings)
                {
                        var contour = new ContourVertex[ringSize];

                        for (int i = pointIndex, contourIndex = 0; i < pointIndex + ringSize; ++i, ++contourIndex)
                        {
                                var point = geometry.points[i];
                                contour[contourIndex].Position = new Vec3 { X = point.x, Y = height, Z = point.y };
                        }

                        pointIndex += ringSize;

                        tess.AddContour(contour, ContourOrientation.Original);
                }

                tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, 3);

                for (int i = 0; i < tess.ElementCount * 3; ++i)
                {
                        meshData.indices.Add(tess.Elements[i]);
                }

                for (int i = 0; i < tess.VertexCount; ++i)
                {
                        var position = tess.Vertices[i].Position;
                        meshData.vertices.Add(new Vector3(position.X, position.Y, position.Z));
                        meshData.colors.Add(color);
                }

                return meshData;
        }

        public static MeshData TesselatePolygonExtrusion(Geometry geometry, Color color, float minHeight, float height)
        {
                var meshData = new MeshData();
                int pointIndex = 0;
                int indexOffset = 0;

                foreach (var ringSize in geometry.rings)
                {
                        for (int i = pointIndex; i < pointIndex + ringSize - 1; i++)
                        {
                                var p0 = geometry.points[i];
                                var p1 = geometry.points[i+1];

                                meshData.vertices.Add(new Vector3(p0.x, height, p0.y));
                                meshData.vertices.Add(new Vector3(p1.x, height, p1.y));
                                meshData.vertices.Add(new Vector3(p0.x, minHeight, p0.y));
                                meshData.vertices.Add(new Vector3(p1.x, minHeight, p1.y));

                                for (int colorIndex = 0; colorIndex < 4; ++colorIndex)
                                {
                                        meshData.colors.Add(color);
                                }

                                meshData.indices.Add(indexOffset + 2);
                                meshData.indices.Add(indexOffset + 3);
                                meshData.indices.Add(indexOffset + 1);
                                meshData.indices.Add(indexOffset + 2);
                                meshData.indices.Add(indexOffset + 1);
                                meshData.indices.Add(indexOffset + 0);

                                indexOffset += 4;
                        }

                        pointIndex += ringSize;
                }

                return meshData;
        }
}
