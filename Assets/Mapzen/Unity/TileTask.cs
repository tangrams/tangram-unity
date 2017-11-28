using System.Collections.Generic;
using Mapzen;
using Mapzen.Unity;
using Mapzen.VectorData;
using Mapzen.VectorData.Formats;
using Mapzen.VectorData.Filters;
using UnityEngine;

public class TileTask
{
    private TileAddress address;
    private byte[] tileData;
    private bool ready;
    private SceneGroupType groupOptions;
    private float inverseTileScale;
    private Matrix4x4 transform;
    private List<FeatureMesh> data;
    private List<MapStyle> featureStyling;
    private int generation;

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

    public TileTask(List<MapStyle> featureStyling, TileAddress address, Matrix4x4 transform, byte[] tileData, int generation)
    {
        this.data = new List<FeatureMesh>();
        this.address = address;
        this.tileData = tileData;
        this.transform = transform;
        this.ready = false;
        this.featureStyling = featureStyling;
        this.generation = generation;
    }

    public void Start()
    {
        float inverseTileScale = 1.0f / (float)address.GetSizeMercatorMeters();

        // TODO: Reuse tile parsing data
        // var tileData = new GeoJsonTile(address, response);
        var mvtTile = new MvtTile(address, tileData);

        foreach (var style in featureStyling)
        {
            if (style == null)
            {
                continue;
            }

            foreach (var filterStyle in style.Layers)
            {
                foreach (var layer in mvtTile.FeatureCollections)
                {

                    foreach (var feature in filterStyle.GetFilter().Filter(layer))
                    {
                        var layerStyle = filterStyle.Style;
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
}
