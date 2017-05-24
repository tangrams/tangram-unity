using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapzen.VectorData.Filters
{
    public class FeatureFilter : IFeatureFilter
    {
        public IFeatureMatcher Matcher { get; set; }

        public List<string> CollectionNameSet { get; set; }

        public FeatureFilter()
        {
            Matcher = new FeatureMatcher();
            CollectionNameSet = new List<string>();
        }

        public virtual IEnumerable<Feature> Filter(FeatureCollection collection)
        {
            if (CollectionNameSet.Contains(collection.Name))
            {
                return collection.Features.Where(Matcher.MatchesFeature);
            }
            return Enumerable.Empty<Feature>();
        }

        public FeatureFilter TakeAllFromCollections(params string[] collectionNames)
        {
            CollectionNameSet.AddRange(collectionNames);
            return this;
        }

        public FeatureFilter Where(IFeatureMatcher predicate)
        {
            Matcher = predicate;
            return this;
        }
    }
}
