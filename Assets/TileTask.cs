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

    public void Start(Dictionary<IFeatureFilter, Material> featureStyling)
    {
        // Parse the GeoJSON
        // var tileData = new GeoJsonTile(address, response);
        var tileData = new MvtTile(address, response);

        float inverseTileScale = 1.0f / (float)address.GetSizeMercatorMeters();

        foreach (var entry in featureStyling)
        {
            var filter = entry.Key;
            var material = entry.Value;

            foreach (var layer in tileData.FeatureCollections)
            {
                var filteredFeatures = filter.Filter(layer);

                foreach (var feature in filteredFeatures)
                {
                    var options = new PolygonBuilder.Options();
                    options.Material = material;

                    object heightValue;
                    if (feature.TryGetProperty("height", out heightValue) && heightValue is double)
                    {
                        // For some reason we can't cast heightValue straight to float.
                        options.MaxHeight = (float)((double)heightValue * inverseTileScale);
                        options.Extrude = true;
                    }

                    if (feature.Type == GeometryType.Polygon)
                    {
                        var builder = new PolygonBuilder(Data, options);
                        feature.HandleGeometry(builder);
                    }

                    if (feature.Type == GeometryType.LineString)
                    {
                        var polylineOptions = new PolylineBuilder.Options();
                        polylineOptions.Material = material;
                        polylineOptions.Width = (float)(5.0 * inverseTileScale);
                        polylineOptions.Extrude = true;
                        polylineOptions.MaxHeight = (float)(3.0 * inverseTileScale);

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
