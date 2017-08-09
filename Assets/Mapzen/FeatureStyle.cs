using System;
using Mapzen.VectorData.Filters;
using Mapzen.VectorData;
using UnityEngine;

namespace Mapzen
{
    public class FeatureStyle
    {
        public IFeatureFilter Filter
        {
            get;
            internal set;
        }

        public Material Material
        {
            get;
            internal set;
        }

        public PolygonBuilder.Options polygonBuilderOptions;
        private PolylineBuilder.Options polylineBuilderOptions;

        public FeatureStyle(IFeatureFilter filter, Material material,
                            PolygonBuilder.Options polygonBuilderOptions,
                            PolylineBuilder.Options polylineBuilderOptions)
        {
            this.Filter = filter;
            this.Material = material;
            this.polygonBuilderOptions = polygonBuilderOptions;
            this.polylineBuilderOptions = polylineBuilderOptions;
        }

        public PolygonBuilder.Options PolygonOptions(Feature feature, float inverseTileScale)
        {
            var options = polygonBuilderOptions;

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

        public PolylineBuilder.Options PolylineOptions(Feature feature, float inverseTileScale)
        {
            var options = polylineBuilderOptions;

            options.Material = this.Material;
            options.Width *= inverseTileScale;
            options.MaxHeight *= inverseTileScale;

            return options;
        }
    }
}
