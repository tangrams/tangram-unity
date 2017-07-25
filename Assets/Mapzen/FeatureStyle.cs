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
            if (feature.Type != GeometryType.Polygon && feature.Type != GeometryType.MultiPolygon)
            {
                return null;
            }

            var polygonOptions = new PolygonBuilder.Options();

            object heightValue;
            if (feature.TryGetProperty("height", out heightValue) && heightValue is double)
            {
                polygonOptions.MaxHeight = (float)((double)heightValue * inverseTileScale);
                polygonOptions.Extrude = true;
            }

            polygonOptions.Material = this.Material;

            return polygonOptions;
        }

        public PolylineBuilder.Options PolylineOptions(Feature feature, float inverseTileScale)
        {
            if (feature.Type != GeometryType.LineString && feature.Type != GeometryType.MultiLineString)
            {
                return null;
            }

            var polylineOptions = new PolylineBuilder.Options();
            polylineOptions.Material = this.Material;
            polylineOptions.Width = 15.0f * inverseTileScale;
            polylineOptions.Extrude = true;
            polylineOptions.MaxHeight = 3.0f * inverseTileScale;
            polylineOptions.MiterLimit = 3.0f;

            return polylineOptions;
        }
    }
}
