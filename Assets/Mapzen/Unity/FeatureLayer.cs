using Mapzen.VectorData.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mapzen.Unity
{
    [Serializable]
    public class FeatureLayer
    {
        public enum MapzenFeatureCollection
        {
            Boundaries,
            Buildings,
            Earth,
            Landuse,
            Places,
            POIs,
            Roads,
            Transit,
            Water,
        }

        public enum MatcherCombiner
        {
            None,
            AllOf,
            AnyOf,
            NoneOf,
        }

        public string Name;

        public MapzenFeatureCollection FeatureCollection;

        public MatcherCombiner Combiner;

        public List<LayerMatcher> Matchers;

        public LayerStyle Style;

        public FeatureLayer(string name)
        {
            Name = name;
        }

        public FeatureFilter GetFilter()
        {
            var filter = new FeatureFilter();
            filter.CollectionNameSet.Add(FeatureCollection.ToString().ToLower());

            if (Matchers == null || Matchers.Count == 0)
            {
                return filter;
            }
            else
            {
                IFeatureMatcher[] predicates = Matchers.Select(m => m.GetFeatureMatcher()).ToArray();
                switch (Combiner)
                {
                    case MatcherCombiner.AllOf:
                        filter.Matcher = FeatureMatcher.AllOf(predicates);
                        break;
                    case MatcherCombiner.AnyOf:
                        filter.Matcher = FeatureMatcher.AnyOf(predicates);
                        break;
                    case MatcherCombiner.NoneOf:
                        filter.Matcher = FeatureMatcher.NoneOf(predicates);
                        break;
                    default:
                        break;
                }
            }

            return filter;
        }
    }
}
