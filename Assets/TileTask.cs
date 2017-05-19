using System;
using System.Collections.Generic;
using Mapzen;
// TODO remove
using UnityEngine;
using Mapzen.VectorData;

public class TileTask {
        public TileAddress address;
        public List<FeatureCollection> features;
        private string response;
        private MapTile tile;
        private List<string> layers;
        private bool ready;

        // TODO: remove those offset once we have better tile placement
        public float offsetX = 0.0f;
        public float offsetY = 0.0f;

        public TileTask(TileAddress address, List<string> layers, string response, MapTile tile)
        {
                this.address = address;
                this.response = response;
                this.layers = layers;
                this.tile = tile;

                ready = false;
        }

        public void Start()
        {
                var projection = GeoJSON.LocalCoordinateProjectionForTile(address);

                // Parse the GeoJSON
                var geoJson = new GeoJSON(response, projection);

                features = geoJson.GetLayersByName (layers);
                // Tesselate the mesh
                // tile.BuildMesh(address.GetSizeMercatorMeters(), geoJson.GetLayersByName(layers));

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