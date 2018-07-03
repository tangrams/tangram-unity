using Nextzen.VectorData.Filters;
using System;
using UnityEngine;

namespace Nextzen.Unity
{
    [Serializable]
    public class LayerMatcher
    {
        public enum Kind
        {
            None,
            Property,
            PropertyRange,
            PropertyRegex,
            PropertyValue,
        }

        public Kind MatcherKind;

        public string PropertyKey = "";
        public string PropertyValue = "";
        public string RegexPattern = "";
        public float MinRange;
        public float MaxRange;
        public bool MinRangeEnabled = true;
        public bool MaxRangeEnabled = true;

        public LayerMatcher(Kind type)
        {
            MatcherKind = type;
        }

        public IFeatureMatcher GetFeatureMatcher()
        {
            IFeatureMatcher matcher = new FeatureMatcher();

            switch (MatcherKind)
            {
                case LayerMatcher.Kind.PropertyRange:
                    double? min = MinRangeEnabled ? (double)MinRange : (double?)null;
                    double? max = MaxRangeEnabled ? (double)MaxRange : (double?)null;
                    matcher = FeatureMatcher.HasPropertyInRange(PropertyKey, min, max);
                    break;

                case LayerMatcher.Kind.Property:
                    matcher = FeatureMatcher.HasProperty(PropertyKey);
                    break;

                case LayerMatcher.Kind.PropertyValue:
                    matcher = FeatureMatcher.HasPropertyWithValue(PropertyKey, PropertyValue);
                    break;

                case LayerMatcher.Kind.PropertyRegex:
                    try
                    {
                        matcher = FeatureMatcher.HasPropertyWithRegex(PropertyKey, RegexPattern);
                    }
                    catch (ArgumentException ae)
                    {
                        Debug.LogError(ae.Message);
                    }
                    break;
            }

            return matcher;
        }
    }
}