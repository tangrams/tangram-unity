using System;
using System.Collections.Generic;

namespace Mapzen.VectorData.Filters
{
    [Serializable]
    public class PropertyValueFeatureMatcher : PropertyFeatureMatcher
    {
        public List<object> ValueSet;

        protected override bool MatchesProperty(object property)
        {
            return ValueSet.Contains(property);
        }
    }
}
