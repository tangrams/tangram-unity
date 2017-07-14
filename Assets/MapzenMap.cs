using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using Mapzen.VectorData;
using Mapzen.VectorData.Filters;
using Mapzen;

public class MapzenMap : MonoBehaviour
{
    public string ApiKey = "vector-tiles-tyHL4AY";

    public TileArea Area = new TileArea(new LngLat(-74.014892578125, 40.70562793820589), new LngLat(-74.00390625, 40.713955826286046), 16);
    private List<GameObject> tiles = new List<GameObject>();

    [SerializeField]
    private string exportPath = "Assets/Generated";

    private UnityIO tileIO = new UnityIO();

    [SerializeField]
    private Dictionary<IFeatureFilter, Material> featureStyling = new Dictionary<IFeatureFilter, Material>();

    public void DownloadTiles()
    {
        GameObject tilePrefab = Resources.Load("Tile") as GameObject;

        /*
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
        */

        TileBounds bounds = new TileBounds(Area);

        foreach (var tileAddress in bounds.TileAddressRange)
        {
            var wrappedTileAddress = tileAddress.Wrapped();
            var uri = new Uri(string.Format("https://tile.mapzen.com/mapzen/vector/v1/all/{0}/{1}/{2}.mvt?api_key={3}",
                          wrappedTileAddress.z, wrappedTileAddress.x, wrappedTileAddress.y, ApiKey));

            Debug.Log("URL request " + uri.AbsoluteUri);

            UnityIO.IORequestCallback onTileFetched = (response) =>
            {
                if (response.hasError())
                {
                    Debug.Log("TileIO Error: " + response.error);
                    return;
                }

                if (response.data.Length == 0)
                {
                    Debug.Log("Empty Response");
                    return;
                }

                // Adding a tile object to the scene
                GameObject tilePrefab = Resources.Load("Tile") as GameObject;

                // Instantiate a prefab running the script TileData.Start()
                var go = Instantiate(tilePrefab);

                go.name = tileAddress.ToString();
                go.transform.parent = this.transform;

                MapTile tile = go.GetComponent<MapTile>();

                float offsetX = (tileAddress.x - TileX);
                float offsetY = (-tileAddress.y + TileY);

                TileTask task = new TileTask(tileAddress, response, offsetX, offsetY);
                task.Start(featureStyling);
                tile.CreateUnityMesh(task.Data, offsetX, offsetY);

                tiles.Add(go);
            };

            // Starts the HTTP request
            StartCoroutine(tileIO.FetchNetworkData(uri, onTileFetched));
        }
    }

    public List<GameObject> Tiles
    {
        get
        {
            return tiles;
        }
    }


    public Dictionary<IFeatureFilter, Material> FeatureStyling
    {
        get { return featureStyling; }
    }

    public string ExportPath
    {
        get
        {
            return exportPath;
        }
        set
        {
            exportPath = value;
        }
    }
}
