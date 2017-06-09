using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mapzen.VectorData;
using Mapzen.VectorData.Filters;
using SimpleJSON;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapTile : MonoBehaviour
{
    private MeshData meshData = new MeshData();

    private Dictionary<IFeatureFilter, Material> featureStyling = new Dictionary<IFeatureFilter, Material>();

    public void Start()
    {
        // Filter that accepts all features in the "water" layer.
        var waterLayerFilter = new FeatureFilter().TakeAllFromCollections("water");

        // Filter that accepts all features in the "buildings" layer with a "height" property.
        var buildingExtrusionFilter = new FeatureFilter().TakeAllFromCollections("buildings").Where(FeatureMatcher.HasPropertyInRange("height", null, 30));

        // Filter that accepts all features in the "earth" or "landuse" layers.
        var landLayerFilter = new FeatureFilter().TakeAllFromCollections("earth", "landuse");

        var baseMaterial = GetComponent<MeshRenderer>().material;

        var waterMaterial = new Material(baseMaterial);
        waterMaterial.color = Color.blue;

        var buildingMaterial = new Material(baseMaterial);
        buildingMaterial.color = Color.gray;

        var landMaterial = new Material(baseMaterial);
        landMaterial.color = Color.green;

        featureStyling.Add(waterLayerFilter, waterMaterial);
        featureStyling.Add(buildingExtrusionFilter, buildingMaterial);
        featureStyling.Add(landLayerFilter, landMaterial);
    }

    public void BuildMesh(double tileScale, List<FeatureCollection> layers)
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
                    // TODO: use extrusion scale and minHeight as options
                    float height = 0.0f;
                    float minHeight = 0.0f;

                    object heightValue;
                    if (feature.TryGetProperty("height", out heightValue) && heightValue is double)
                    {
                        // For some reason we can't cast heightValue straight to float.
                        height = (float)((double)heightValue * inverseTileScale);
                    }

                    if (feature.geometry.type == GeometryType.Polygon)
                    {
                        Builder.TesselatePolygon(meshData, feature.geometry, material, height);

                        if (height > 0.0f)
                        {
                            Builder.TesselatePolygonExtrusion(meshData, feature.geometry, material, minHeight, height);
                        }
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
