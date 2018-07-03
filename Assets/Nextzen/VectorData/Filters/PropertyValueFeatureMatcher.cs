using System;
using System.Collections.Generic;

namespace Nextzen.VectorData.Filters
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