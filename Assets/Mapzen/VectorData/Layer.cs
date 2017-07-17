using System.Collections.Generic;
using Mapzen.VectorData.Filters;

namespace Mapzen.VectorData
{
    public class Layer
    {
        public string Name { get; set; }

        public FeatureFilter Filter { get; set; }

        public List<Layer> Sublayers { get; set; }



    }
}

