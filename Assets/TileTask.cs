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
    private SceneGroup.Type groupOptions;

    public float OffsetX { get; internal set; }

    public float OffsetY { get; internal set; }

    public TileTask(TileAddress address, SceneGroup.Type groupOptions, byte[] response, float offsetX, float offsetY)
    {
        this.address = address;
        this.response = response;
        this.ready = false;
        this.groupOptions = groupOptions;
        this.OffsetX = offsetX;
        this.OffsetY = offsetY;
    }

    public void Start(List<FeatureStyle> featureStyling, SceneGroup root)
    {
        // Parse the GeoJSON
        // var tileData = new GeoJsonTile(address, response);
        var tileData = new MvtTile(address, response);

        SceneGroup leaf = root;
        SceneGroup parent = root;

        float inverseTileScale = 1.0f / (float)address.GetSizeMercatorMeters();

        OnSceneGroupData(SceneGroup.Type.Tile, address.ToString(), ref parent, ref leaf);

        foreach (var style in featureStyling)
        {
            OnSceneGroupData(SceneGroup.Type.Filter, style.Name, ref parent, ref leaf);

            foreach (var layer in tileData.FeatureCollections)
            {
                OnSceneGroupData(SceneGroup.Type.Layer, layer.Name, ref parent, ref leaf);

                foreach (var feature in style.Filter.Filter(layer))
                {
                    string featureName = "feature";
                    object nameProperty;

                    if (feature.TryGetProperty("name", out nameProperty))
                    {
                        featureName = (string)nameProperty;
                    }

                    OnSceneGroupData(SceneGroup.Type.Feature, featureName, ref parent, ref leaf);

                    if (feature.Type == GeometryType.Polygon || feature.Type == GeometryType.MultiPolygon)
                    {
                        var polygonOptions = style.PolygonOptions(feature, inverseTileScale);
                        var builder = new PolygonBuilder(leaf.meshData, polygonOptions);

                        feature.HandleGeometry(builder);
                    }

                    if (feature.Type == GeometryType.LineString || feature.Type == GeometryType.MultiLineString)
                    {
                        var polylineOptions = style.PolylineOptions(feature, inverseTileScale);
                        var builder = new PolylineBuilder(leaf.meshData, polylineOptions);

                        feature.HandleGeometry(builder);
                    }
                }
            }
        }

        ready = true;
    }

    private void OnSceneGroupData(SceneGroup.Type type, string name, ref SceneGroup parent, ref SceneGroup leaf)
    {
        if (SceneGroup.Test(type, groupOptions))
        {
            var group = new SceneGroup(type, name);

            if (parent != null)
                parent.childs.Add(group);

            if (SceneGroup.IsLeaf(type, groupOptions))
                leaf = group;
            else
                parent = group;
        }
    }

    public bool IsReady()
    {
        return ready;
    }
}
