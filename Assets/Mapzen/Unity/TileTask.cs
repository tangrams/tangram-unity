using System.Collections.Generic;
using Mapzen;
using Mapzen.Unity;
using Mapzen.VectorData;
using Mapzen.VectorData.Filters;
using UnityEngine;

public class TileTask
{
    private TileAddress address;
    private bool ready;
    private Matrix4x4 transform;
    private int generation;
    private List<FeatureMesh> data;
    private List<MapStyle> featureStyling;

    public int Generation
    {
        get { return generation; }
    }

    public List<FeatureMesh> Data
    {
        get { return data; }
    }

    public bool Ready
    {
        get { return ready; }
    }

    public TileTask(List<MapStyle> featureStyling, TileAddress address, Matrix4x4 transform, int generation)
    {
        this.data = new List<FeatureMesh>();
        this.address = address;
        this.transform = transform;
        this.ready = false;
        this.generation = generation;
        this.featureStyling = featureStyling;
    }

    public void Start(IEnumerable<FeatureCollection> featureCollections)
    {
        float inverseTileScale = 1.0f / (float)address.GetSizeMercatorMeters();

        foreach (var style in featureStyling)
        {
            if (style == null)
            {
                continue;
            }

            foreach (var styleLayer in style.Layers)
            {
                foreach (var collection in featureCollections)
                {

                    foreach (var feature in styleLayer.GetFilter().Filter(collection))
                    {
                        var layerStyle = styleLayer.Style;
                        string featureName = "";
                        object identifier;

                        if (feature.TryGetProperty("id", out identifier))
                        {
                            featureName += identifier.ToString();
                        }

                        FeatureMesh featureMesh = new FeatureMesh(address.ToString(), collection.Name, styleLayer.Name, featureName);

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
}
