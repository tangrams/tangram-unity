using System;
using System.Collections.Generic;
using Mapzen;
using Mapzen.VectorData;
using Mapzen.VectorData.Formats;
using Mapzen.VectorData.Filters;
using UnityEngine;

public class TileTask
{
    private TileAddress address;
    private byte[] response;
    private bool ready;

    public float OffsetX { get; internal set; }

    public float OffsetY { get; internal set; }

    public Dictionary<string, MeshData> Data { get; internal set; }

    public TileTask(TileAddress address, byte[] response, float offsetX, float offsetY)
    {
        this.address = address;
        this.response = response;
        this.Data = new Dictionary<string, MeshData>();
        this.OffsetX = offsetX;
        this.OffsetY = offsetY;
        ready = false;
    }

    public void Start(List<FeatureStyle> featureStyling)
    {
        // Parse the GeoJSON
        // var tileData = new GeoJsonTile(address, response);
        var tileData = new MvtTile(address, response);

        float inverseTileScale = 1.0f / (float)address.GetSizeMercatorMeters();

        foreach (var style in featureStyling)
        {
            var filter = style.Filter;
            var meshData = new MeshData();
            Data.Add(style.Name, meshData);

            foreach (var layer in tileData.FeatureCollections)
            {
                var filteredFeatures = filter.Filter(layer);

                foreach (var feature in filteredFeatures)
                {
                    if (feature.Type == GeometryType.Polygon || feature.Type == GeometryType.MultiPolygon)
                    {
                        var polygonOptions = style.PolygonOptions(feature, inverseTileScale);
                        var builder = new PolygonBuilder(meshData, polygonOptions);
                        feature.HandleGeometry(builder);
                    }

                    if (feature.Type == GeometryType.LineString || feature.Type == GeometryType.MultiLineString)
                    {
                        var polylineOptions = style.PolylineOptions(feature, inverseTileScale);
                        var builder = new PolylineBuilder(meshData, polylineOptions);
                        feature.HandleGeometry(builder);
                    }
                }
            }

            meshData.FlipIndices();
        }

        ready = true;
    }

    public bool IsReady()
    {
        return ready;
    }
}
