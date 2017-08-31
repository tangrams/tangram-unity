using System;
using Mapzen.VectorData.Filters;
using Mapzen.VectorData;
using UnityEngine;

namespace Mapzen
{
    [Serializable]
    public class FeatureStyle
    {

        public FeatureFilter Filter;
        public Material Material;
        public string Name;
        public PolygonBuilder.Options PolygonBuilderOptions;
        public PolylineBuilder.Options PolylineBuilderOptions;

        public FeatureStyle(FeatureFilter filter,
                            Material material, string name,
                            PolygonBuilder.Options polygonBuilderOptions,
                            PolylineBuilder.Options polylineBuilderOptions)
        {
            this.Filter = filter;
            this.Name = name;
            this.Material = material;
            this.PolygonBuilderOptions = polygonBuilderOptions;
            this.PolylineBuilderOptions = polylineBuilderOptions;
        }

        public PolygonBuilder.Options GetPolygonOptions(Feature feature, float inverseTileScale)
        {
            var options = PolygonBuilderOptions;

            options.Material = this.Material;

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

        public PolylineBuilder.Options GetPolylineOptions(Feature feature, float inverseTileScale)
        {
            var options = PolylineBuilderOptions;

            options.Material = this.Material;
            options.Width *= inverseTileScale;
            options.MaxHeight *= inverseTileScale;

            return options;
        }
    }
}
