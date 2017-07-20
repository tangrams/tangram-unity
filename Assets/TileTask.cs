using System;
using System.Collections.Generic;
using Mapzen;
using Mapzen.VectorData;
using Mapzen.VectorData.Formats;

public class TileTask
{
    private TileAddress address;
    private byte[] response;
    private MapTile tile;
    private bool ready;

    // TODO: remove those offset once we have better tile placement
    public float offsetX = 0.0f;
    public float offsetY = 0.0f;

    public TileTask(TileAddress address, byte[] response, MapTile tile)
    {
        this.address = address;
        this.response = response;
        this.tile = tile;

        ready = false;
    }

    public void Start()
    {

        // Parse the GeoJSON
        // var tileData = new GeoJsonTile(address, response);
        var tileData = new MvtTile(address, response);

        // Tesselate the mesh
        tile.BuildMesh(address.GetSizeMercatorMeters(), tileData.FeatureCollections);

        ready = true;
    }

    public bool IsReady()
    {
        return ready;
    }

    public MapTile GetMapTile()
    {
        return tile;
    }
}
