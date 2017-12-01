using System.Collections.Generic;
using Mapzen;
using Mapzen.Unity;
using Mapzen.VectorData;
using Mapzen.VectorData.Filters;
using UnityEngine;

public class TileTask
{
    // The tile address this task is working on
    private TileAddress address;
    // Whether the tile task has finished its work
    private bool ready;
    // The transform applied to the geometry built the tile task builders
    private Matrix4x4 transform;
    // The generation of this tile task
    private int generation;
    // The resulting data of the tile task is stored in this container
    private List<FeatureMesh> data;
    // The map styling this tile task is working on
    private List<MapStyle> featureStyling;

    public int Generation
    {
        get { return generation; }
    }

    public List<FeatureMesh> Data
    {
        get { return data; }
    }

    public TileTask(List<MapStyle> featureStyling, TileAddress address, Matrix4x4 transform, int generation)
    {
        this.data = new List<FeatureMesh>();
        this.address = address;
        this.transform = transform;
        this.generation = generation;
        this.featureStyling = featureStyling;
    }

    /// <summary>
    /// Runs the tile task, resulting data will be stored in Data.
    /// </summary>
    /// <param name="featureCollections">The feature collections this tile task will be building.</param>
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

                        // Resulting data for this feature.
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
    }
}
