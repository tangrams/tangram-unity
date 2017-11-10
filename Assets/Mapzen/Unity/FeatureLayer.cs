using Mapzen.VectorData.Filters;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mapzen.Unity
{
    [Serializable]
    public class FeatureLayer
    {
        public string Name;

        public LayerStyle Style;

        public LayerMatcher Matcher;

        public List<string> FeatureCollections;

        public FeatureLayer(string name)
        {
            Name = name;
        }

        public FeatureFilter GetFilter()
        {
            var filter = new FeatureFilter();
            filter.Matcher = Matcher.GetFeatureMatcher();
            foreach (var collection in FeatureCollections)
            {
                filter.CollectionNameSet.Add(collection);
            }
            return filter;
        }
    }
}
