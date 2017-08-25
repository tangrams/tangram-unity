using System;
using System.Linq;
using System.Collections.Generic;
using Mapzen.VectorData.Filters;
using Mapzen.VectorData;
using UnityEngine;

namespace Mapzen
{
    [Serializable]
    public class FeatureStyle
    {
        [Serializable]
        public class LayerStyle
        {
            [SerializeField]
            private string layerName;

            public Material Material;
            public bool EnablePolygonBuilder;
            public bool EnablePolylineBuilder;
            public PolygonBuilder.Options PolygonBuilderOptions;
            public PolylineBuilder.Options PolylineBuilderOptions;

            public string LayerName
            {
                get { return layerName; }
            }

            public LayerStyle(string layerName)
            {
                this.layerName = layerName;
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

        [Serializable]
        public class FilterStyle
        {
            [SerializeField]
            private string name;

            [SerializeField]
            private List<LayerStyle> layerStyles;

            [SerializeField]
            private FeatureFilter filter;

            public List<LayerStyle> LayerStyles
            {
                get { return layerStyles; }
            }

            public FeatureFilter Filter
            {
                get { return filter; }
                set { filter = value; }
            }

            public string Name
            {
                get { return name; }
            }

            public FilterStyle(string name)
            {
                this.name = name;
                this.layerStyles = new List<LayerStyle>();
                this.filter = new FeatureFilter();
            }

            public void AddLayerStyle(LayerStyle layerStyle)
            {
                layerStyles.Add(layerStyle);
            }
        }

        [SerializeField]
        private List<FilterStyle> filterStyles;

        [SerializeField]
        private string name;

        public List<FilterStyle> FilterStyles
        {
            get { return filterStyles; }
        }

        public string Name
        {
            get { return name; }
        }

        public FeatureStyle(string name)
        {
            this.filterStyles = new List<FilterStyle>();
            this.name = name;
        }

        public bool AddFilterStyle(FilterStyle filterStyle)
        {
            var queryFilterStyleName = filterStyles.Where(fs => fs.Name == filterStyle.Name);

            if (queryFilterStyleName.Count() > 0)
            {
                return false;
            }

            filterStyles.Add(filterStyle);
            return true;
        }
    }
}
