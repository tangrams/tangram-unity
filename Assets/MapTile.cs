using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mapzen;
using Mapzen.VectorData;
using Mapzen.VectorData.Filters;
using SimpleJSON;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapTile : MonoBehaviour
{
    private MeshData meshData = new MeshData();

    private Dictionary<IFeatureFilter, Material> featureStyling = new Dictionary<IFeatureFilter, Material>();

    public void Awake()
    {
        // Filter that accepts all features in the "water" layer.
        var waterLayerFilter = new FeatureFilter().TakeAllFromCollections("water");

        // Filter that accepts all features in the "buildings" layer with a "height" property.
        var buildingExtrusionFilter = new FeatureFilter().TakeAllFromCollections("buildings");

        // Filter that accepts all features in the "earth" or "landuse" layers.
        var landLayerFilter = new FeatureFilter().TakeAllFromCollections("earth", "landuse");

        var minorRoadLayerFilter = new FeatureFilter().TakeAllFromCollections("roads").Where(FeatureMatcher.HasPropertyWithValue("kind", "minor_road"));
        var highwayRoadLayerFilter = new FeatureFilter().TakeAllFromCollections("roads").Where(FeatureMatcher.HasPropertyWithValue("kind", "highway"));

        var baseMaterial = GetComponent<MeshRenderer>().material;

        var waterMaterial = new Material(baseMaterial);
        waterMaterial.color = Color.blue;

        var buildingMaterial = new Material(baseMaterial);
        buildingMaterial.color = Color.gray;

        var landMaterial = new Material(baseMaterial);
        landMaterial.color = Color.green;

        var minorRoadsMaterial = new Material(baseMaterial);
        minorRoadsMaterial.color = Color.white;

        var highwayRoadsMaterial = new Material(baseMaterial);
        highwayRoadsMaterial.color = Color.black;

        featureStyling.Add(waterLayerFilter, waterMaterial);
        featureStyling.Add(buildingExtrusionFilter, buildingMaterial);
        featureStyling.Add(landLayerFilter, landMaterial);
        featureStyling.Add(minorRoadLayerFilter, minorRoadsMaterial);
        featureStyling.Add(highwayRoadLayerFilter, highwayRoadsMaterial);
    }

    public void BuildMesh(double tileScale, IEnumerable<FeatureCollection> layers)
    {
        float inverseTileScale = 1.0f / (float)tileScale;

        foreach (var entry in featureStyling)
        {
            var filter = entry.Key;
            var material = entry.Value;

            foreach (var layer in layers)
            {
                var filteredFeatures = filter.Filter(layer);

                foreach (var feature in filteredFeatures)
                {
                    var options = new PolygonBuilder.Options();
                    options.Material = material;

                    object heightValue;
                    if (feature.TryGetProperty("height", out heightValue) && heightValue is double)
                    {
                        // For some reason we can't cast heightValue straight to float.
                        options.MaxHeight = (float)((double)heightValue * inverseTileScale);
                        options.Extrude = true;
                    }

                    if (feature.Type == GeometryType.Polygon)
                    {
                        var builder = new PolygonBuilder(meshData, options);
                        feature.HandleGeometry(builder);
                    }

                    if (feature.Type == GeometryType.LineString)
                    {
                        var polylineOptions = new PolylineBuilder.Options();
                        polylineOptions.Material = material;
                        polylineOptions.Width = (float)(5.0 * inverseTileScale);
                        polylineOptions.Extrude = true;
                        polylineOptions.MaxHeight = (float)(3.0 * inverseTileScale);

                        var builder = new PolylineBuilder(meshData, polylineOptions);
                        feature.HandleGeometry(builder);
                    }
                }
            }
        }

        meshData.FlipIndices();
    }

    public void CreateUnityMesh(float offsetX, float offsetY)
    {
        var mesh = new Mesh();

        mesh.SetVertices(meshData.Vertices);

        mesh.subMeshCount = meshData.Submeshes.Count;
        for (int i = 0; i < meshData.Submeshes.Count; i++)
        {
            mesh.SetTriangles(meshData.Submeshes[i].Indices, i);
        }

        mesh.RecalculateNormals();

        transform.Translate(new Vector3(offsetX, 0.0f, offsetY));

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().materials = meshData.Submeshes.Select(s => s.Material).ToArray();
    }

    public void Update()
    {
        //transform.Rotate(Vector3.up, Time.deltaTime * 10.0f);
    }
}
