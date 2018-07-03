using SimpleJSON;
using System.Collections.Generic;

namespace Nextzen.VectorData.Formats
{
    using LocalCoordinateProjection = GeoJsonTile.LocalCoordinateProjection;

    public class GeoJsonFeatureCollection : FeatureCollection
    {
        private string name;
        private JSONNode featureCollectionNode;
        private LocalCoordinateProjection projection;

        public GeoJsonFeatureCollection(string name, JSONNode node, LocalCoordinateProjection projection)
        {
            this.name = name;
            this.featureCollectionNode = node;
            this.projection = projection;
        }

        public override string Name
        {
            get { return name; }
        }

        public override IEnumerable<Feature> Features
        {
            get
            {
                var featuresArray = featureCollectionNode["features"].AsArray;
                foreach (var featureNode in featuresArray.Children)
                {
                    yield return new GeoJsonFeature(featureNode, projection);
                }
            }
        }
    }
}