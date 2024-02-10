using System;
using System.Text.RegularExpressions;

namespace Mapzen.VectorData.Filters
{
    [Serializable]
    public class PropertyRegexFeatureMatcher : PropertyFeatureMatcher
    {
        public Regex Regex;

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
