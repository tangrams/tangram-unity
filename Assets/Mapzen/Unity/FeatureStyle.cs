using System;
using System.Linq;
using System.Collections.Generic;
using Mapzen.VectorData.Filters;
using Mapzen.VectorData;
using UnityEngine;

namespace Mapzen.Unity
{
    [Serializable]
    [CreateAssetMenu(menuName = "Mapzen/Style")]
    public class FeatureStyle : ScriptableObject
    {
        [Serializable]
        public class PolygonBuilderEditorOption {
            public Guid editorGUID;
            public PolygonBuilder.Options option;
        }

        [Serializable]
        public class PolylineBuilderEditorOption {
            public Guid editorGUID;
            public PolylineBuilder.Options option;
        }

        [Serializable]
        public class LayerStyle
        {
            [SerializeField]
            private string layerName;

            public List<PolygonBuilderEditorOption> PolygonBuilderEditorOptions;
            public List<PolylineBuilderEditorOption> PolylineBuilderEditorOptions;

            public string LayerName
            {
                get { return layerName; }
            }

            public LayerStyle(string layerName)
            {
                this.layerName = layerName;
                this.PolygonBuilderEditorOptions = new List<PolygonBuilderEditorOption>();
                this.PolylineBuilderEditorOptions = new List<PolylineBuilderEditorOption>();
            }

            public List<PolygonBuilder.Options> GetPolygonOptions(Feature feature, float inverseTileScale)
            {
                List<PolygonBuilder.Options> polygonBuilderOptions = new List<PolygonBuilder.Options>();

                foreach (var editorOption in PolygonBuilderEditorOptions) {
                    var options = editorOption.option;

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

                    polygonBuilderOptions.Add(options);
                }

                return polygonBuilderOptions;
            }

            public List<PolylineBuilder.Options> GetPolylineOptions(Feature feature, float inverseTileScale)
            {
                List<PolylineBuilder.Options> polylineBuilderOptions = new List<PolylineBuilder.Options>();

                foreach (var editorOption in PolylineBuilderEditorOptions) {
                    var options = editorOption.option;

                    options.Width *= inverseTileScale;
                    options.MaxHeight *= inverseTileScale;

                    polylineBuilderOptions.Add(options);
                }

                return polylineBuilderOptions;
            }
        }

        [Serializable]
        public class Matcher
        {
            public enum Type
            {
                None,
                AllOf,
                NoneOf,
                AnyOf,
                Property,
                PropertyRange,
                PropertyRegex,
                PropertyValue,
            }

            [SerializeField]
            private Type type;

            [SerializeField]
            private List<Matcher> matchers;

            public string HasProperty = "";
            public string PropertyValue = "";
            public string RegexPattern = "";
            public float MinRange;
            public float MaxRange;
            public bool MinRangeEnabled = true;
            public bool MaxRangeEnabled = true;

            public bool IsCompound()
            {
                return type == Type.AllOf || type == Type.NoneOf || type == Type.AnyOf;
            }

            public List<Matcher> Matchers
            {
                get { return matchers; }
            }

            public IFeatureMatcher GetFeatureMatcher()
            {
                IFeatureMatcher matcher = new FeatureMatcher();

                if (IsCompound() && matchers.Count > 0)
                {
                    var predicates = new List<IFeatureMatcher>();

                    for (int i = 0; i < matchers.Count; ++i)
                    {
                        var predicate = matchers[i].GetFeatureMatcher();
                        if (predicate != null)
                        {
                            predicates.Add(predicate);
                        }
                    }

                    switch (type)
                    {
                        case FeatureStyle.Matcher.Type.AllOf:
                            matcher = FeatureMatcher.AllOf(predicates.ToArray());
                            break;
                        case FeatureStyle.Matcher.Type.NoneOf:
                            matcher = FeatureMatcher.NoneOf(predicates.ToArray());
                            break;
                        case FeatureStyle.Matcher.Type.AnyOf:
                            matcher = FeatureMatcher.AnyOf(predicates.ToArray());
                            break;
                    }
                }
                else
                {
                    switch (type)
                    {
                        case FeatureStyle.Matcher.Type.PropertyRange:
                            double? min = MinRangeEnabled ? (double)MinRange : (double?)null;
                            double? max = MaxRangeEnabled ? (double)MaxRange : (double?)null;

                            matcher = FeatureMatcher.HasPropertyInRange(HasProperty, min, max);
                            break;
                        case FeatureStyle.Matcher.Type.Property:
                            matcher = FeatureMatcher.HasProperty(HasProperty);
                            break;
                        case FeatureStyle.Matcher.Type.PropertyValue:
                            matcher = FeatureMatcher.HasPropertyWithValue(HasProperty, PropertyValue);
                            break;
                        case FeatureStyle.Matcher.Type.PropertyRegex:
                            try
                            {
                                matcher = FeatureMatcher.HasPropertyWithRegex(HasProperty, RegexPattern);
                            }
                            catch (ArgumentException ae)
                            {
                                Debug.LogError(ae.Message);
                            }
                            break;
                    }
                }

                return matcher;
            }

            public Type MatcherType
            {
                get { return type; }
            }

            public Matcher(Type type)
            {
                this.matchers = new List<Matcher>();
                this.type = type;
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

            public Matcher Matcher;

            public List<LayerStyle> LayerStyles
            {
                get { return layerStyles; }
            }

            public FeatureFilter GetFilter()
            {
                var filter = new FeatureFilter();
                filter.Matcher = Matcher.GetFeatureMatcher();
                foreach (var layerStyle in layerStyles)
                {
                    filter.CollectionNameSet.Add(layerStyle.LayerName);
                }
                return filter;
            }

            public string Name
            {
                get { return name; }
            }

            public FilterStyle(string name)
            {
                this.name = name;
                this.layerStyles = new List<LayerStyle>();
            }
        }

        [SerializeField]
        private List<FilterStyle> filterStyles;

        public List<FilterStyle> FilterStyles
        {
            get { return filterStyles; }
        }

        #if UNITY_EDITOR
        public object Editor;
        #endif

        public FeatureStyle()
        {
            this.filterStyles = new List<FilterStyle>();
        }

        public void AddFilterStyle(FilterStyle filterStyle)
        {
            filterStyles.Add(filterStyle);
        }

        public void RemoveFilterStyle(FilterStyle filterStyle)
        {
            filterStyles.Remove(filterStyle);
        }
    }
}
