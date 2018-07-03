using System;
using System.Collections.Generic;
using System.Linq;

namespace Nextzen.VectorData.Filters
{
    [Serializable]
    public class CompoundFeatureMatcher : IFeatureMatcher
    {
        public enum Operator
        {
            Any,
            All,
            None,
        }

        public List<IFeatureMatcher> Matchers;

        public Operator Type;

        public bool MatchesFeature(Feature feature)
        {
            switch (Type)
            {
                case Operator.Any:
                    return Matchers.Any(m => m.MatchesFeature(feature));

                case Operator.All:
                    return Matchers.All(m => m.MatchesFeature(feature));

                case Operator.None:
                    return !Matchers.Any(m => m.MatchesFeature(feature));

                default:
                    return false;
            }
        }
    }
}