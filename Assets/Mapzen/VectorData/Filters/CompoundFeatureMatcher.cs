using System.Collections.Generic;
using System.Linq;

namespace Mapzen.VectorData.Filters
{
    public class CompoundFeatureMatcher : IFeatureMatcher
    {
        public enum Operator
        {
            Any,
            All,
            None,
        }

        public List<IFeatureMatcher> Matchers { get; set; }

        public Operator Type { get; set; }

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

