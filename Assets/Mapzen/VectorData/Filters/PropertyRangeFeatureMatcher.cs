using System;

namespace Mapzen.VectorData.Filters
{
    public class PropertyRangeFeatureMatcher : PropertyFeatureMatcher
    {
        public IComparable Min { get; set; }

        public IComparable Max { get; set; }

        protected override bool MatchesProperty(object property)
        {
            // If a Min value is set and the property value precedes it, return false.
            if (Min != null && Min.CompareTo(property) > 0)
            {
                return false;
            }
            // If a Max value is set and it precedes the property value, return false.
            if (Max != null && Max.CompareTo(property) <= 0)
            {
                return false;
            }
            // Otherwise, return true.
            return true;
        }
    }
}
