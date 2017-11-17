using System;
using System.Collections.Generic;
using Mapzen;
using Mapzen.Unity;
using Mapzen.VectorData;
using Mapzen.VectorData.Formats;
using Mapzen.VectorData.Filters;
using UnityEngine;
using System.Linq;

public class TileTask
{
    private TileAddress address;
    private byte[] response;
    private bool ready;
    private SceneGroupType groupOptions;
    private float inverseTileScale;
    private Matrix4x4 transform;

    public TileTask(TileAddress address, SceneGroupType groupOptions, byte[] response, float offsetX, float offsetY, float regionScaleRatio)
    {
        this.address = address;
        this.response = response;
        this.ready = false;
        this.groupOptions = groupOptions;
        this.inverseTileScale = 1.0f / (float)address.GetSizeMercatorMeters();

        float scaleRatio = (float)address.GetSizeMercatorMeters() * regionScaleRatio;
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(scaleRatio, scaleRatio, scaleRatio));
        Matrix4x4 translate = Matrix4x4.Translate(new Vector3(offsetX * scaleRatio, 0.0f, offsetY * scaleRatio));
        this.transform = translate * scale;
    }

    public void Start(List<MapStyle> featureStyling, SceneGroup root)
    {
        // Parse the GeoJSON
        // var tileData = new GeoJsonTile(address, response);
        var tileData = new MvtTile(address, response);

        // The leaf currently used (will hold the mesh data for the currently matched group)
        SceneGroup leaf = root;

        var tileGroup = OnSceneGroupData(SceneGroupType.Tile, address.ToString(), root, ref leaf);

        foreach (var style in featureStyling)
        {
            if (style == null)
            {
                continue;
            }

            foreach (var filterStyle in style.Layers)
            {
                var filterGroup = OnSceneGroupData(SceneGroupType.Filter, filterStyle.Name, tileGroup, ref leaf);

                foreach (var layer in tileData.FeatureCollections)
                {
                    var layerGroup = OnSceneGroupData(SceneGroupType.Layer, layer.Name, filterGroup, ref leaf);

                    foreach (var feature in filterStyle.GetFilter().Filter(layer))
                    {
                        var layerStyle = filterStyle.Style;
                        string featureName = "";
                        object identifier;

                        if (feature.TryGetProperty("id", out identifier))
                        {
                            featureName += identifier.ToString();
                        }

                        OnSceneGroupData(SceneGroupType.Feature, featureName, layerGroup, ref leaf);

                        if (feature.Type == GeometryType.Polygon || feature.Type == GeometryType.MultiPolygon)
                        {
                            var polygonOptions = layerStyle.GetPolygonOptions(feature, inverseTileScale);

                            if (polygonOptions.Enabled)
                            {
                                var builder = new PolygonBuilder(leaf.meshData, polygonOptions, transform);
                                feature.HandleGeometry(builder);
                            }
                        }

                        if (feature.Type == GeometryType.LineString || feature.Type == GeometryType.MultiLineString)
                        {
                            var polylineOptions = layerStyle.GetPolylineOptions(feature, inverseTileScale);

                            if (polylineOptions.Enabled)
                            {
                                var builder = new PolylineBuilder(leaf.meshData, polylineOptions, transform);
                                feature.HandleGeometry(builder);
                            }
                        }
                    }
                }
            }
        }

        ready = true;
    }

    private SceneGroup OnSceneGroupData(SceneGroupType type, string name, SceneGroup parent, ref SceneGroup leaf)
    {
        SceneGroup group = null;

        if (SceneGroup.Test(type, groupOptions))
        {
            foreach (var child in parent.children)
            {
                if (child.name == name)
                {
                    group = child;
                }
            }

            // No group found for this idenfier
            if (group == null)
            {
                group = new SceneGroup(type, name);
                parent.children.Add(group);
            }

            // Update the leaf
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
