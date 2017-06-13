using System.Collections.Generic;

namespace Mapzen.VectorData
{
    public class FeatureCollection
    {
        public FeatureCollection(string name = "")
        {
            Name = name;
            Features = new List<Feature>();
        }

        public string Name;
        public List<Feature> Features;
    }
}
