using Mapzen.VectorData.Filters;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mapzen.Unity
{
    [Serializable]
    public class LayerMatcher
    {
        public enum Kind
        {
            None,
            AllOf,
            NoneOf,
            AnyOf,
            Property,
            PropertyRange,
            PropertyRegex,
            PropertyValue,
        }

        public Kind MatcherKind;

        public List<LayerMatcher> Matchers = new List<LayerMatcher>();

        public string HasProperty = "";
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

        public bool IsCompound()
        {
            return MatcherKind == Kind.AllOf || MatcherKind == Kind.NoneOf || MatcherKind == Kind.AnyOf;
        }

        public IFeatureMatcher GetFeatureMatcher()
        {
            IFeatureMatcher matcher = new FeatureMatcher();

            if (IsCompound() && Matchers.Count > 0)
            {
                var predicates = new List<IFeatureMatcher>();

                for (int i = 0; i < Matchers.Count; ++i)
                {
                    var predicate = Matchers[i].GetFeatureMatcher();
                    if (predicate != null)
                    {
                        predicates.Add(predicate);
                    }
                }

                switch (MatcherKind)
                {
                    case LayerMatcher.Kind.AllOf:
                        matcher = FeatureMatcher.AllOf(predicates.ToArray());
                        break;
                    case LayerMatcher.Kind.NoneOf:
                        matcher = FeatureMatcher.NoneOf(predicates.ToArray());
                        break;
                    case LayerMatcher.Kind.AnyOf:
                        matcher = FeatureMatcher.AnyOf(predicates.ToArray());
                        break;
                }
            }
            else
            {
                switch (MatcherKind)
                {
                    case LayerMatcher.Kind.PropertyRange:
                        double? min = MinRangeEnabled ? (double)MinRange : (double?)null;
                        double? max = MaxRangeEnabled ? (double)MaxRange : (double?)null;

                        matcher = FeatureMatcher.HasPropertyInRange(HasProperty, min, max);
                        break;
                    case LayerMatcher.Kind.Property:
                        matcher = FeatureMatcher.HasProperty(HasProperty);
                        break;
                    case LayerMatcher.Kind.PropertyValue:
                        matcher = FeatureMatcher.HasPropertyWithValue(HasProperty, PropertyValue);
                        break;
                    case LayerMatcher.Kind.PropertyRegex:
                        try
                        {
                            matcher = FeatureMatcher.HasPropertyWithRegex(HasProperty, RegexPattern);
                        }
                        catch (ArgumentException ae)
                        {
                            Debug.LogError(ae.Message);
                        }
                        break;
                }
            }

            return matcher;
        }
    }
}
