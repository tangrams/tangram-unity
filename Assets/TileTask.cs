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
    private float inverseTileScale;
    private Matrix4x4 transform;
    private bool isStaticGameObject = true;
    private bool hasCollider = true;

    public TileTask(TileAddress address, SceneGroup.Type groupOptions, byte[] response, float offsetX, float offsetY, float regionScaleRatio)
    {
        this.address = address;
        this.response = response;
        this.ready = false;
        this.groupOptions = groupOptions;
        this.inverseTileScale = 1.0f / (float)address.GetSizeMercatorMeters();
        float val = (float)address.GetSizeMercatorMeters() * regionScaleRatio;
        this.transform = Matrix4x4.Translate(new Vector3(offsetX * val, 0.0f, offsetY * val)) * Matrix4x4.Scale(new Vector3(val, val, val));
    }

    public void Start(List<FeatureStyle> featureStyling, SceneGroup root)
    {
        // Parse the GeoJSON
        // var tileData = new GeoJsonTile(address, response);
        var tileData = new MvtTile(address, response);

        // The leaf currently used (will hold the mesh data for the currently matched group)
        SceneGroup leaf = root;

        var tileGroup = OnSceneGroupData(SceneGroup.Type.Tile, address.ToString(), root, ref leaf);

        foreach (var style in featureStyling)
        {
            isStaticGameObject = style.IsStatic;
            hasCollider = style.HasCollider;

            var filterGroup = OnSceneGroupData(SceneGroup.Type.Filter, style.Name, tileGroup, ref leaf);

            foreach (var layer in tileData.FeatureCollections)
            {
                var layerGroup = OnSceneGroupData(SceneGroup.Type.Layer, layer.Name, filterGroup, ref leaf);

                foreach (var feature in style.Filter.Filter(layer))
                {
                    string featureName = "";
                    object identifier;

                    if (feature.TryGetProperty("id", out identifier))
                    {
                        featureName += identifier.ToString();
                    }

                    OnSceneGroupData(SceneGroup.Type.Feature, featureName, layerGroup, ref leaf);

                    if (feature.Type == GeometryType.Polygon || feature.Type == GeometryType.MultiPolygon)
                    {
                        var polygonOptions = style.GetPolygonOptions(feature, inverseTileScale);
                        var builder = new PolygonBuilder(leaf.meshData, polygonOptions, transform);

                        feature.HandleGeometry(builder);
                    }

                    if (feature.Type == GeometryType.LineString || feature.Type == GeometryType.MultiLineString)
                    {
                        var polylineOptions = style.GetPolylineOptions(feature, inverseTileScale);
                        var builder = new PolylineBuilder(leaf.meshData, polylineOptions, transform);

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

            // No group found for this idenfier
            if (group == null)
            {
                group = new SceneGroup(type, name, isStaticGameObject);
                parent.childs[name] = group;
            }

            // Update the leaf
            if (SceneGroup.IsLeaf(type, groupOptions))
            {
                leaf = group;
                leaf.hasCollider = hasCollider;
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
