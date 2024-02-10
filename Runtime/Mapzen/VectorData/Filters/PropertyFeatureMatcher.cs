using System;

namespace Mapzen.VectorData.Filters
{
    [Serializable]
    public class PropertyFeatureMatcher : IFeatureMatcher
    {
        public string Key;

        public bool MatchesFeature(Feature feature)
        {
            object property;
            if (feature.TryGetProperty(Key, out property))
            {
                return MatchesProperty(property);
            }
            return false;
        }

        protected virtual bool MatchesProperty(object property)
        {
            return true;
        }
    }
}
