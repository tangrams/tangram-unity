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
    private SceneGroup.Type groupOptions;
    private float inverseTileScale;
    private Matrix4x4 transform;
    private List<FeatureMesh> data;

    public List<FeatureMesh> Data
    {
        get { return data; }
    }

    public TileTask(TileAddress address, SceneGroup.Type groupOptions, byte[] response, float offsetX, float offsetY, float regionScaleRatio)
    {
        this.data = new List<FeatureMesh>();
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

    public void Start(List<FeatureStyle> featureStyling)
    {
        // TODO: Reuse tile parsing data
        // var tileData = new GeoJsonTile(address, response);
        var tileData = new MvtTile(address, response);

        foreach (var style in featureStyling)
        {
            if (style == null)
            {
                continue;
            }

            foreach (var filterStyle in style.FilterStyles)
            {
                foreach (var layer in tileData.FeatureCollections)
                {
                    foreach (var feature in filterStyle.GetFilter().Filter(layer))
                    {
                        var layerStyle = filterStyle.LayerStyles.Find(ls => ls.LayerName == layer.Name);

                        string featureName = "";
                        object identifier;

                        if (feature.TryGetProperty("id", out identifier))
                        {
                            featureName += identifier.ToString();
                        }

                        FeatureMesh featureMesh = new FeatureMesh(address.ToString(), layer.Name, filterStyle.Name, featureName);

                        IGeometryHandler handler = null;

                        if (feature.Type == GeometryType.Polygon || feature.Type == GeometryType.MultiPolygon)
                        {
                            var polygonOptions = layerStyle.GetPolygonOptions(feature, inverseTileScale);

                            if (polygonOptions.Enabled)
                            {
                                handler = new PolygonBuilder(featureMesh.Mesh, polygonOptions, transform);
                            }
                        }

                        if (feature.Type == GeometryType.LineString || feature.Type == GeometryType.MultiLineString)
                        {
                            var polylineOptions = layerStyle.GetPolylineOptions(feature, inverseTileScale);

                            if (polylineOptions.Enabled)
                            {
                                handler = new PolylineBuilder(featureMesh.Mesh, polylineOptions, transform);
                            }
                        }

                        if (handler != null)
                        {
                            feature.HandleGeometry(handler);
                            data.Add(featureMesh);
                        }
                    }
                }
            }
        }

        ready = true;
    }

    public bool IsReady()
    {
        return ready;
    }
}
