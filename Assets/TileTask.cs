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

    public List<GameObject> GameObjects { get; internal set; }

    public TileTask(TileAddress address, byte[] response, float offsetX, float offsetY)
    {
        this.address = address;
        this.response = response;
        this.GameObjects = new List<GameObject>();
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

            GameObject filterGameObject = new GameObject(style.Name);

            List<GameObject> childs = new List<GameObject>();

            foreach (var layer in tileData.FeatureCollections)
            {
                List<Feature> filteredFeatures = new List<Feature>(filter.Filter(layer));

                GameObject layerGameObject = null;

                if (filteredFeatures.Count > 0)
                {
                    layerGameObject = new GameObject(layer.Name);
                    layerGameObject.transform.parent = filterGameObject.transform;
                    childs.Add(layerGameObject);
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
                        // NOTE: when dispatching task from work threads, implement RunOnMainThread()
                        // to dispatch the following in main thread
                        GameObject go = new GameObject();

                        object name;
                        if (feature.TryGetProperty("name", out name))
                        {
                            go.name = (string)name;
                        }

                        go.transform.parent = layerGameObject.transform;
                        FeatureBehavior featureBehavior = go.AddComponent<FeatureBehavior>();
                        featureBehavior.CreateUnityMesh(meshData, OffsetX, OffsetY);
                    }
                }
            }

            if (childs.Count > 0)
            {
                GameObjects.Add(filterGameObject);
            }
        }

        ready = true;
    }

    public bool IsReady()
    {
        return ready;
    }
}
