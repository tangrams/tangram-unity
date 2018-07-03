using System;

namespace Nextzen.VectorData.Filters
{
    [Serializable]
    public class PropertyRangeFeatureMatcher : PropertyFeatureMatcher
    {
        public double? Min;

        public double? Max;

        protected override bool MatchesProperty(object property)
        {
            var number = property as double?;
            if (number == null)
            {
                return false;
            }
            // If a Min value is set and the property value precedes it, return false.
            if (Min != null && number < Min)
            {
                return false;
            }
            // If a Max value is set and it precedes the property value, return false.
            if (Max != null && number >= Max)
            {
                return false;
            }
            // Otherwise, return true.
            return true;
        }
    }
}