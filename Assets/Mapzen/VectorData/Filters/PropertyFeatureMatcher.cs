namespace Mapzen.VectorData.Filters
{
    public class PropertyFeatureMatcher : IFeatureMatcher
    {
        public string Key { get; set; }

        public bool MatchesFeature(Feature feature)
        {
            object property;
            if (feature.properties.TryGetValue(Key, out property))
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
