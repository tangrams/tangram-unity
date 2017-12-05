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

            if (options.MaxHeight == 0.0f)
            {
                object heightValue;
                if (feature.TryGetProperty("height", out heightValue) && heightValue is double)
                {
                    options.MaxHeight = (float)(double)heightValue;
                }
            }

            if (options.MinHeight == 0.0f)
            {
                object heightValue;
                if (feature.TryGetProperty("min_height", out heightValue) && heightValue is double)
                {
                    options.MinHeight = (float)(double)heightValue;
                }
            }

            options.MaxHeight *= inverseTileScale;
            options.MinHeight *= inverseTileScale;

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
