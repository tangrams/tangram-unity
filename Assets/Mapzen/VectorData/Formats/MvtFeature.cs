using System;
using System.Collections.Generic;
using Mvtcs;

namespace Mapzen.VectorData.Formats
{
    using PbfFeature = Mvtcs.Tile.Types.Feature;
    using PbfLayer = Mvtcs.Tile.Types.Layer;
    using PbfGeomType = Mvtcs.Tile.Types.GeomType;
    using PbfValueType = Mvtcs.Tile.Types.Value.KindOneofCase;

    public class MvtFeature : Feature
    {
        private PbfLayer layer;
        private PbfFeature feature;

        public MvtFeature(PbfLayer layer, PbfFeature feature)
        {
            this.layer = layer;
            this.feature = feature;
        }

        public override bool TryGetProperty(string key, out object value)
        {
            value = null;
            int nTags = feature.Tags.Count;
            for (int i = 0; i < nTags; i += 2)
            {
                var propertyKeyIndex = (int)feature.Tags[i];
                var propertyKey = layer.Keys[propertyKeyIndex];
                if (propertyKey == key)
                {
                    var propertyValueIndex = (int)feature.Tags[i + 1];
                    var propertyValue = layer.Values[propertyValueIndex];
                    switch (propertyValue.KindCase)
                    {
                        case PbfValueType.BoolValue:
                            value = propertyValue.BoolValue;
                            break;
                        case PbfValueType.IntValue:
                            value = (double)propertyValue.IntValue;
                            break;
                        case PbfValueType.UintValue:
                            value = (double)propertyValue.UintValue;
                            break;
                        case PbfValueType.SintValue:
                            value = (double)propertyValue.SintValue;
                            break;
                        case PbfValueType.FloatValue:
                            value = (double)propertyValue.FloatValue;
                            break;
                        case PbfValueType.DoubleValue:
                            value = propertyValue.DoubleValue;
                            break;
                        case PbfValueType.StringValue:
                            value = propertyValue.StringValue;
                            break;
                        case PbfValueType.None:
                            value = null;
                            break;
                    }
                    return true;
                }
            }
            return false;
        }

        public static GeometryType TypeFromPbfType(Mvtcs.Tile.Types.GeomType pbfType)
        {
            switch (pbfType)
            {
                case PbfGeomType.Point: return GeometryType.Point;
                case PbfGeomType.LineString: return GeometryType.LineString;
                case PbfGeomType.Polygon: return GeometryType.Polygon;
                case PbfGeomType.Unknown: return GeometryType.Unknown;
            }
            return GeometryType.Unknown;
        }

        public override GeometryType Type
        {
            get
            {
                return TypeFromPbfType(feature.Type);
            }
        }

        protected enum CommandType : uint
        {
            MoveTo = 1,
            LineTo = 2,
            ClosePath = 7,
        }

        protected struct GeometryDecoder
        {
            // Feature geometry is encoded in a buffer of uints.
            Google.Protobuf.Collections.RepeatedField<uint> data;
            int index;

            // Geometry is encoded in integer positions from 0 to extent-1.
            uint extent;
            float scale;

            // A cursor position tracks the state of geometry decoding.
            long x;
            long y;

            CommandType command;
            int repeat;

            public CommandType Command { get { return command; } }

            public int Repeat { get { return repeat; } }

            public GeometryDecoder(PbfFeature feature, PbfLayer layer)
            {
                data = feature.Geometry;
                index = 0;
                extent = layer.Extent;
                scale = 1.0f / (extent - 1);
                x = 0;
                y = 0;
                command = CommandType.MoveTo;
                repeat = 0;
            }

            public bool AdvanceCommand()
            {
                if (index >= data.Count)
                {
                    return false;
                }
                uint cmd = data[index++];
                // The 3 lowest bits of the command encode the type, the rest are the repeat count.
                command = (CommandType)(cmd & 0x7);
                repeat = (int)(cmd >> 3);
                return true;
            }

            public bool AdvanceCursor()
            {
                if (index + 1 >= data.Count)
                {
                    return false;
                }
                // For each MoveTo and LineTo repetition there are 2 parameter integers.
                uint param0 = data[index++];
                uint param1 = data[index++];
                // The parameters are zigzag-encoded deltas for x and y of the cursor.
                x += ((param0 >> 1) ^ (-(param0 & 1)));
                y += ((param1 >> 1) ^ (-(param1 & 1)));
                return true;
            }

            public Point CurrentPoint()
            {
                return new Point(x * scale, (extent - y) * scale);
            }
        }

        public override bool HandleGeometry(IGeometryHandler handler)
        {
            var decoder = new GeometryDecoder(feature, layer);

            // From https://github.com/mapbox/vector-tile-spec/tree/master/2.1
            //
            // The POINT geometry type encodes a point or multipoint geometry. The geometry command sequence for a point
            // geometry MUST consist of a single MoveTo command with a command count greater than 0.
            //
            // The LINESTRING geometry type encodes a linestring or multilinestring geometry. The geometry command sequence
            // for a linestring geometry MUST consist of one or more repetitions of the following sequence:
            //   1. A MoveTo command with a command count of 1
            //   2. A LineTo command with a command count greater than 0
            //
            // The POLYGON geometry type encodes a polygon or multipolygon geometry, each polygon consisting of exactly one
            // exterior ring that contains zero or more interior rings. The geometry command sequence for a polygon consists
            // of one or more repetitions of the following sequence:
            //   1. An ExteriorRing
            //   2. Zero or more InteriorRings
            // Each ExteriorRing and InteriorRing MUST consist of the following sequence:
            //   1. A MoveTo command with a command count of 1
            //   2. A LineTo command with a command count greater than 1
            //   3. A ClosePath command

            switch (feature.Type)
            {
                case PbfGeomType.Point:
                    if (decoder.AdvanceCommand() && decoder.Command == CommandType.MoveTo)
                    {
                        for (int i = 0; i < decoder.Repeat; i++)
                        {
                            decoder.AdvanceCursor();
                            handler.OnPoint(decoder.CurrentPoint());
                        }
                    }
                    break;
                case PbfGeomType.LineString:
                    while (decoder.AdvanceCommand() && (decoder.Command == CommandType.MoveTo && decoder.Repeat == 1))
                    {
                        decoder.AdvanceCursor();
                        if (decoder.AdvanceCommand() && (decoder.Command == CommandType.LineTo && decoder.Repeat > 0))
                        {
                            handler.OnBeginLineString();
                            handler.OnPoint(decoder.CurrentPoint());
                            for (int i = 0; i < decoder.Repeat; i++)
                            {
                                decoder.AdvanceCursor();
                                handler.OnPoint(decoder.CurrentPoint());
                            }
                            handler.OnEndLineString();
                        }
                    }
                    break;
                case PbfGeomType.Polygon:
                    while (decoder.AdvanceCommand() && (decoder.Command == CommandType.MoveTo && decoder.Repeat == 1))
                    {
                        handler.OnBeginPolygon();
                        decoder.AdvanceCursor();
                        if (decoder.AdvanceCommand() && (decoder.Command == CommandType.LineTo && decoder.Repeat > 0))
                        {
                            handler.OnBeginLinearRing();
                            var start = decoder.CurrentPoint();
                            handler.OnPoint(start);
                            for (int i = 0; i < decoder.Repeat; i++)
                            {
                                decoder.AdvanceCursor();
                                handler.OnPoint(decoder.CurrentPoint());
                            }
                            handler.OnPoint(start);
                            handler.OnEndLinearRing();
                        }
                        if (decoder.AdvanceCommand() && decoder.Command == CommandType.ClosePath)
                        {
                            // We should assert that this condition holds.
                        }
                        handler.OnEndPolygon();
                    }
                    break;
                case PbfGeomType.Unknown:
                    break;
            }
            return true;
        }
    }
}

