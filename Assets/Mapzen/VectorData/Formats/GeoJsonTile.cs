using System;
using System.Collections.Generic;
using SimpleJSON;

namespace Mapzen.VectorData.Formats
{
    public class GeoJsonTile : Tile
    {
        public delegate Point LocalCoordinateProjection(double longitude, double latitude);

        public static LocalCoordinateProjection LocalCoordinateProjectionForTile(TileAddress address)
        {
            var origin = address.GetOriginMercatorMeters();
            var scale = address.GetSizeMercatorMeters();

            return delegate(double longitude, double latitude)
            {
                var lngLat = new LngLat(longitude, latitude);
                var meters = lngLat.ToMercatorMeters();
                var x = (meters.x - origin.x) / scale;
                var y = (meters.y - origin.y) / scale;
                return new Point((float)x, (float)y);
            };
        }

        private JSONNode tileNode;

        private LocalCoordinateProjection projection;

        public GeoJsonTile(TileAddress address, byte[] data)
        {
            this.address = address;
            var dataAsString = System.Text.Encoding.Default.GetString(data);
            tileNode = JSON.Parse(dataAsString);
            projection = LocalCoordinateProjectionForTile(address);
        }

        public override IEnumerable<FeatureCollection> FeatureCollections
        {
            get
            {
                // The root JSON node for a GeoJSON tile may be either a single FeatureCollection
                // or a mapping of names to FeatureCollections. If the root is a FeatureCollection
                // then it will have a key of "type" with a value of "FeatureCollection".
                if (tileNode["type"].Value == "FeatureCollection")
                {
                    yield return new GeoJsonFeatureCollection("", tileNode, projection);
                }
                else
                {
                    foreach (var entry in tileNode.AsObject)
                    {
                        // The Enumerator for JSONObject yields Dictionary entries, but as objects.
                        var pair = (KeyValuePair<string, JSONNode>)entry;
                        yield return new GeoJsonFeatureCollection(pair.Key, pair.Value, projection);
                    }
                }
            }
        }
    }
}
