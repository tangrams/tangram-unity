using System;
using System.Collections.Generic;
using SimpleJSON;
using Mapzen.VectorData;

namespace Mapzen
{
    public class GeoJSON
    {
        public delegate Point LocalCoordinateProjection(LngLat lngLat);

        public static LocalCoordinateProjection LocalCoordinateProjectionForTile(TileAddress address)
        {
            return delegate(LngLat lngLat)
            {
                var meters = lngLat.ToMercatorMeters();
                var origin = address.GetOriginMercatorMeters();
                var scale = address.GetSizeMercatorMeters();
                var x = (meters.x - origin.x) / scale;
                var y = (meters.y - origin.y) / scale;
                return new Point((float)x, (float)y);
            };
        }

        private JSONNode root;
        private LocalCoordinateProjection transform;

        public GeoJSON(string geoJSONDataString, LocalCoordinateProjection transform)
        {
            root = JSON.Parse(geoJSONDataString);
            this.transform = transform;
        }

        public List<FeatureCollection> GetLayersByName(List<string> layerNames)
        {
            if (root == null)
            {
                return null;
            }

            var layers = new List<FeatureCollection>();

            foreach (string name in layerNames)
            {
                JSONNode layerNode = root[name];

                if (layerNode == null)
                {
                    Console.WriteLine("Can't find layer '{0}' in GeoJSON data.", name);
                    continue;
                }

                var layer = GetLayer(layerNode, name);
                layers.Add(layer);
            }
            return layers;
        }

        FeatureCollection GetLayer(JSONNode layerNode, string layerName)
        {
            var layer = new FeatureCollection(layerName);

            JSONNode features = layerNode["features"];

            if (features == null)
            {
                return layer;
            }

            foreach (JSONNode feature in features.Children)
            {
                layer.features.Add(GetFeature(feature));
            }

            return layer;
        }

        Feature GetFeature(JSONNode featureNode)
        {
            Feature feature = new Feature();

            JSONNode propertiesNode = featureNode["properties"];
            if (propertiesNode != null && propertiesNode.IsObject)
            {
                foreach (KeyValuePair<string, JSONNode> property in propertiesNode.AsObject)
                {
                    // TODO: Define more strict typing on property values.
                    feature.properties.Add(property.Key, property.Value);
                }
            }

            JSONNode geometryNode = featureNode["geometry"];
            if (geometryNode != null)
            {
                feature.geometry = GetGeometry(geometryNode);
            }

            return feature;
        }

        Geometry GetGeometry(JSONNode geometryNode)
        {
            JSONNode coords = geometryNode["coordinates"];
            JSONNode type = geometryNode["type"];

            if (type != null && coords != null)
            {
                switch (type.Value)
                {
                    case "Point":
                        return GetPointGeometry(coords);
                    case "MultiPoint":
                        return GetMultiPointGeometry(coords);
                    case "LineString":
                        return GetLineStringGeometry(coords);
                    case "MultiLineString":
                        return GetMultiLineStringGeometry(coords);
                    case "Polygon":
                        return GetPolygonGeometry(coords);
                    case "MultiPolygon":
                        return GetMultiPolygonGeometry(coords);
                }
            }
            return new Geometry();
        }

        Point GetPoint(JSONNode coords)
        {
            var lngLat = new LngLat(coords[0].AsDouble, coords[1].AsDouble);
            return transform(lngLat);
        }

        Geometry GetPointGeometry(JSONNode coords)
        {
            var geometry = new Geometry();
            geometry.type = GeometryType.Point;
            geometry.points.Add(GetPoint(coords));
            return geometry;
        }

        Geometry GetMultiPointGeometry(JSONNode coords)
        {
            var geometry = new Geometry();
            geometry.type = GeometryType.Point;
            foreach (JSONNode pointCoords in coords.Children)
            {
                geometry.points.Add(GetPoint(pointCoords));
            }
            return geometry;
        }

        Geometry GetLineStringGeometry(JSONNode coords)
        {
            var geometry = GetMultiPointGeometry(coords);
            geometry.type = GeometryType.LineString;
            return geometry;
        }

        Geometry GetMultiLineStringGeometry(JSONNode coords)
        {
            var geometry = new Geometry();
            geometry.type = GeometryType.LineString;
            int segmentEndIndex = 0;
            List<int> rings = new List<int>();
            foreach (JSONNode lineStringCoords in coords.Children)
            {
                foreach (JSONNode pointCoords in lineStringCoords.Children)
                {
                    geometry.points.Add(GetPoint(pointCoords));
                }
                rings.Add(geometry.points.Count - segmentEndIndex);
                segmentEndIndex = geometry.points.Count;
            }
            geometry.rings.Add(rings);
            return geometry;
        }

        Geometry GetPolygonGeometry(JSONNode coords)
        {
            var geometry = GetMultiLineStringGeometry(coords);
            geometry.type = GeometryType.Polygon;
            return geometry;
        }

        Geometry GetMultiPolygonGeometry(JSONNode coords)
        {
            var geometry = new Geometry();
            geometry.type = GeometryType.Polygon;
            foreach (JSONNode polygonCoordinates in coords.Children)
            {
                var polygonGeometry = GetPolygonGeometry(polygonCoordinates);
                geometry.rings.AddRange(polygonGeometry.rings);
                geometry.points.AddRange(polygonGeometry.points);
            }
            return geometry;
        }
    }
}

