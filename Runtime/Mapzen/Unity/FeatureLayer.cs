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
        [Flags]
        public enum MapzenFeatureCollection : int
        {
            Boundaries = 1,
            Buildings = 2,
            Earth = 4,
            Landuse = 8,
            Places = 16,
            POIs = 32,
            Roads = 64,
            Transit = 128,
            Water = 256,
        }

        public enum MatcherCombiner
        {
            None,
            AllOf,
            AnyOf,
            NoneOf,
        }

        public string Name;

        public MapzenFeatureCollection FeatureCollection = 0;

        public MatcherCombiner Combiner = 0;

        public List<LayerMatcher> Matchers;

        public LayerStyle Style;

        public FeatureLayer(string name)
        {
            Name = name;
        }

        public FeatureFilter GetFilter()
        {
            var filter = new FeatureFilter();

            foreach (MapzenFeatureCollection collection in Enum.GetValues(typeof(MapzenFeatureCollection)))
            {
                if ((FeatureCollection & collection) != 0)
                {
                    filter.CollectionNameSet.Add(collection.ToString().ToLower());
                }
            }

            if (Matchers == null || Matchers.Count == 0)
            {
                return filter;
            }
            else
            {
                IFeatureMatcher[] predicates = Matchers.Select(m => m.GetFeatureMatcher()).ToArray();
                switch (Combiner)
                {
                    case MatcherCombiner.None:
                        filter.Matcher = predicates.First();
                        break;
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
