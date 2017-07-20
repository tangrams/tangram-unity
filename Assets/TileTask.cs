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

    public MeshData Data { get; internal set; }

    public TileTask(TileAddress address, byte[] response, float offsetX, float offsetY)
    {
        this.address = address;
        this.response = response;
        this.Data = new MeshData();
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

            foreach (var layer in tileData.FeatureCollections)
            {
                var filteredFeatures = filter.Filter(layer);

                foreach (var feature in filteredFeatures)
                {
                    var polygonOptions = style.PolygonOptions(feature, inverseTileScale);
                    if (polygonOptions != null)
                    {
                        var builder = new PolygonBuilder(Data, polygonOptions);
                        feature.HandleGeometry(builder);
                    }

                    var polylineOptions = style.PolylineOptions(feature, inverseTileScale);
                    if (polylineOptions != null)
                    {
                        var builder = new PolylineBuilder(Data, polylineOptions);
                        feature.HandleGeometry(builder);
                    }
                }
            }
        }

        Data.FlipIndices();

        ready = true;
    }

    public bool IsReady()
    {
        return ready;
    }
}
