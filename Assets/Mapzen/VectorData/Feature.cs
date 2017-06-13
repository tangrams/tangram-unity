using System.Collections.Generic;

namespace Mapzen.VectorData
{
    public class Feature
    {
        public Feature()
        {
            geometry = new Geometry();
            properties = new Dictionary<string, object>();
        }

        public Geometry geometry { get; set; }

        public Dictionary<string, object> properties { get; set; }

        public bool TryGetProperty(string key, out object value)
        {
            object propertyValue;
            if (properties.TryGetValue(key, out propertyValue))
            {
                if (propertyValue != null)
                {
                    value = propertyValue;
                    return true;
                }
            }
            value = null;
            return false;
        }
    }
}
