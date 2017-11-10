using Mapzen.VectorData;
using System;
using UnityEngine;

namespace Mapzen.Unity
{
    [Serializable]
    public class LayerStyle
    {
        public PolygonOptions PolygonBuilder;

        public PolylineOptions PolylineBuilder;

        public PolygonOptions GetPolygonOptions(Feature feature, float inverseTileScale)
        {
            var options = PolygonBuilder;

            if (options.MaxHeight > 0.0f)
            {
                options.MaxHeight *= inverseTileScale;
            }
            else
            {
                object heightValue;
                if (feature.TryGetProperty("height", out heightValue) && heightValue is double)
                {
                    options.MaxHeight = (float)((double)heightValue * inverseTileScale);
                }
            }

            return options;
        }

        public PolylineOptions GetPolylineOptions(Feature feature, float inverseTileScale)
        {
            var options = PolylineBuilder;

            options.Width *= inverseTileScale;
            options.MaxHeight *= inverseTileScale;

            return options;
        }
    }
}
