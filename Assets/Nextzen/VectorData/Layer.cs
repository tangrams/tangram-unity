using Nextzen.VectorData.Filters;
using System.Collections.Generic;

namespace Nextzen.VectorData
{
    public class Layer
    {
        public string Name { get; set; }

        public FeatureFilter Filter { get; set; }

        public List<Layer> Sublayers { get; set; }
    }
}