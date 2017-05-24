using System.Collections.Generic;

namespace Mapzen.VectorData.Filters
{
    public class PropertyValueFeatureMatcher : PropertyFeatureMatcher
    {
        public List<object> ValueSet { get; set; }

        protected override bool MatchesProperty(object property)
        {
            return ValueSet.Contains(property);
        }
    }
}
