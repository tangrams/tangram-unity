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

        float inverseTileScale = 1.0f / (float)address.GetSizeMercatorMeters();

        var tileGroup = OnSceneGroupData(SceneGroup.Type.Tile, "Tile_" + address.ToString(), root, ref leaf);

        foreach (var style in featureStyling)
        {
            var filterGroup = OnSceneGroupData(SceneGroup.Type.Filter, "Filter_" + style.Name, tileGroup, ref leaf);

            foreach (var layer in tileData.FeatureCollections)
            {
                var layerGroup = OnSceneGroupData(SceneGroup.Type.Layer, "Layer_" + layer.Name, filterGroup, ref leaf);

                foreach (var feature in style.Filter.Filter(layer))
                {
                    string featureName = "Feature_";
                    object identifier;

                    if (feature.TryGetProperty("id", out identifier))
                    {
                        featureName += identifier.ToString();
                    }

                    OnSceneGroupData(SceneGroup.Type.Feature, featureName, layerGroup, ref leaf);

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

    private SceneGroup OnSceneGroupData(SceneGroup.Type type, string name, SceneGroup parent, ref SceneGroup leaf)
    {
        SceneGroup group = null;

        if (SceneGroup.Test(type, groupOptions))
        {
            if (parent.childs.ContainsKey(name))
            {
                group = parent.childs[name];
            }

            if (group == null)
            {
                group = new SceneGroup(type, name);
                parent.childs[name] = group;
            }

            if (SceneGroup.IsLeaf(type, groupOptions))
            {
                leaf = group;
            }
        }
        else
        {
            group = parent;
        }

        return group;
    }

    public bool IsReady()
    {
        return ready;
    }
}
