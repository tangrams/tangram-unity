using SimpleJSON;

namespace Nextzen.VectorData.Formats
{
    using LocalCoordinateProjection = GeoJsonTile.LocalCoordinateProjection;

    public class GeoJsonFeature : Feature
    {
        private JSONObject featureNode;
        private JSONObject propertiesNode;
        private JSONObject geometryNode;
        private GeometryType type;
        private LocalCoordinateProjection projectToLocalPoint;

        public GeoJsonFeature(JSONNode node, LocalCoordinateProjection project)
        {
            featureNode = node.AsObject;
            propertiesNode = featureNode["properties"].AsObject;
            geometryNode = featureNode["geometry"].AsObject;
            type = TypeFromString(geometryNode["type"].Value);
            projectToLocalPoint = project;
        }

        public override bool TryGetProperty(string key, out object value)
        {
            JSONNode property = propertiesNode[key];
            switch (property.Tag)
            {
                case JSONNodeType.NullValue:
                    {
                        value = null;
                        return true;
                    }
                case JSONNodeType.Number:
                    {
                        value = property.AsDouble;
                        return true;
                    }
                case JSONNodeType.String:
                    {
                        value = property.Value;
                        return true;
                    }
                default:
                    {
                        value = null;
                        return false;
                    }
            }
        }

        public static GeometryType TypeFromString(string typeString)
        {
            switch (typeString)
            {
                case "Point":
                    return GeometryType.Point;

                case "MultiPoint":
                    return GeometryType.MultiPoint;

                case "LineString":
                    return GeometryType.LineString;

                case "MultiLineString":
                    return GeometryType.MultiLineString;

                case "Polygon":
                    return GeometryType.Polygon;

                case "MultiPolygon":
                    return GeometryType.MultiPolygon;
            }
            return GeometryType.Unknown;
        }

        public override GeometryType Type
        {
            get { return type; }
        }

        public override bool HandleGeometry(IGeometryHandler handler)
        {
            var coordinates = geometryNode["coordinates"].AsArray;
            switch (Type)
            {
                case GeometryType.Point:
                    HandlePoint(coordinates, handler);
                    break;

                case GeometryType.MultiPoint:
                    foreach (var point in coordinates.Children)
                    {
                        HandlePoint(point.AsArray, handler);
                    }
                    break;

                case GeometryType.LineString:
                    HandleLineString(coordinates, handler);
                    break;

                case GeometryType.MultiLineString:
                    foreach (var lineString in coordinates.Children)
                    {
                        HandleLineString(lineString.AsArray, handler);
                    }
                    break;

                case GeometryType.Polygon:
                    HandlePolygon(coordinates, handler);
                    break;

                case GeometryType.MultiPolygon:
                    foreach (var polygon in coordinates.Children)
                    {
                        HandlePolygon(polygon.AsArray, handler);
                    }
                    break;

                case GeometryType.Unknown:
                    break;
            }
            return true;
        }

        protected void HandlePoint(JSONArray pointCoordinates, IGeometryHandler handler)
        {
            var point = projectToLocalPoint(pointCoordinates[0].AsDouble, pointCoordinates[1].AsDouble);
            handler.OnPoint(point);
        }

        protected void HandleLineString(JSONArray lineStringCoordinates, IGeometryHandler handler)
        {
            handler.OnBeginLineString();
            foreach (var pointCoordinates in lineStringCoordinates.Children)
            {
                HandlePoint(pointCoordinates.AsArray, handler);
            }
            handler.OnEndLineString();
        }

        protected void HandlePolygon(JSONArray polygonCoordinates, IGeometryHandler handler)
        {
            handler.OnBeginPolygon();
            foreach (var linearRingCoordinates in polygonCoordinates.Children)
            {
                handler.OnBeginLinearRing();
                foreach (var pointCoordinates in linearRingCoordinates.Children)
                {
                    HandlePoint(pointCoordinates.AsArray, handler);
                }
                handler.OnEndLinearRing();
            }
            handler.OnEndPolygon();
        }
    }
}