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

    public List<SceneGroup> SceneGroups { get; internal set; }

    public TileTask(TileAddress address, byte[] response, float offsetX, float offsetY)
    {
        this.address = address;
        this.response = response;
        this.SceneGroups = new List<SceneGroup>();
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

            var filterGroup = new SceneGroup(style.Name, null);

            foreach (var layer in tileData.FeatureCollections)
            {
                var filteredFeatures = new List<Feature>(filter.Filter(layer));

                SceneGroup layerGroup = null;

                if (filteredFeatures.Count > 0)
                {
                    layerGroup = new SceneGroup(layer.Name, null);
                    filterGroup.childs.Add(layerGroup);
                }

                foreach (var feature in filteredFeatures)
                {
                    var meshData = new MeshData();

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

                    meshData.FlipIndices();

                    if (meshData.Vertices.Count > 0)
                    {
                        string featureName = "feature";
                        object nameProperty;

                        if (feature.TryGetProperty("name", out nameProperty))
                        {
                            featureName = (string)nameProperty;
                        }

                        var featureGroup = new SceneGroup(featureName, meshData);
                        layerGroup.childs.Add(featureGroup);
                    }
                }
            }

            SceneGroups.Add(filterGroup);
        }

        ready = true;
    }

    public bool IsReady()
    {
        return ready;
    }
}
