using System;
using System.Text.RegularExpressions;

namespace Mapzen.VectorData.Filters
{
    public class PropertyRegexFeatureMatcher : PropertyFeatureMatcher
    {
        public Regex Regex { get; set; }

        protected override bool MatchesProperty(object property)
        {
            string s = property as string;
            if (s != null)
            {
                return Regex.IsMatch(s);
            }
            return false;
        }
    }
}
