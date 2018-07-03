using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR

using UnityEngine;

#else
using System.Diagnostics;
#endif

namespace Nextzen.VectorData.Formats
{
    using PbfFeature = Mvtcs.Tile.Types.Feature;
    using PbfGeomType = Mvtcs.Tile.Types.GeomType;
    using PbfLayer = Mvtcs.Tile.Types.Layer;
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
            public Google.Protobuf.Collections.RepeatedField<uint> Data;

            public int Position;

            // Geometry is encoded in integer positions from 0 to extent-1.
            public uint Extent;

            public float Scale;

            // A cursor position tracks the state of geometry decoding.
            public long X;

            public long Y;

            public CommandType Command;
            public int Repeat;

            public GeometryDecoder(PbfFeature feature, PbfLayer layer)
            {
                Data = feature.Geometry;
                Position = 0;
                Extent = layer.Extent;
                Scale = 1.0f / (Extent - 1);
                X = 0;
                Y = 0;
                Command = CommandType.MoveTo;
                Repeat = 0;
            }

            public void AdvanceCommand()
            {
                uint commandData = Data[Position++];
                // The 3 lowest bits of the command encode the type, the rest are the repeat count.
                Command = (CommandType)(commandData & 0x7);
                Repeat = (int)(commandData >> 3);
            }

            public Point AdvanceCursor()
            {
                // For each MoveTo and LineTo repetition there are 2 parameter integers.
                uint param0 = Data[Position++];
                uint param1 = Data[Position++];
                // The parameters are zigzag-encoded deltas for x and y of the cursor.
                X += ((param0 >> 1) ^ (-(param0 & 1)));
                Y += ((param1 >> 1) ^ (-(param1 & 1)));
                // The coordinates are normalized and Y is flipped to match our expected axes.
                return new Point(X * Scale, (Extent - Y) * Scale);
            }

            public bool HasData()
            {
                return Position < Data.Count;
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
                    while (decoder.HasData())
                    {
                        decoder.AdvanceCommand();
                        Debug.Assert(decoder.Command == CommandType.MoveTo && decoder.Repeat > 0);
                        for (int i = decoder.Repeat; i > 0; i--)
                        {
                            handler.OnPoint(decoder.AdvanceCursor());
                        }
                    }
                    break;

                case PbfGeomType.LineString:
                    while (decoder.HasData())
                    {
                        handler.OnBeginLineString();
                        decoder.AdvanceCommand();
                        Debug.Assert(decoder.Command == CommandType.MoveTo && decoder.Repeat == 1);
                        handler.OnPoint(decoder.AdvanceCursor());
                        decoder.AdvanceCommand();
                        Debug.Assert(decoder.Command == CommandType.LineTo && decoder.Repeat > 0);
                        for (int i = decoder.Repeat; i > 0; i--)
                        {
                            handler.OnPoint(decoder.AdvanceCursor());
                        }
                        handler.OnEndLineString();
                    }
                    break;

                case PbfGeomType.Polygon:
                    // Create temporary storage to hold rings until we determine whether they are interior or exterior.
                    var ring = new List<Point>();
                    var isPolygonStarted = false;
                    while (decoder.HasData())
                    {
                        // Read out the coordinates of the next ring.
                        decoder.AdvanceCommand();
                        Debug.Assert(decoder.Command == CommandType.MoveTo && decoder.Repeat == 1);
                        ring.Add(decoder.AdvanceCursor());
                        decoder.AdvanceCommand();
                        for (int i = decoder.Repeat; i > 0; i--)
                        {
                            ring.Add(decoder.AdvanceCursor());
                        }
                        ring.Add(ring.First());
                        decoder.AdvanceCommand();
                        Debug.Assert(decoder.Command == CommandType.ClosePath && decoder.Repeat == 1);
                        // If ring is exterior, end the current polygon, start a new one, add ring to new polygon.
                        // If ring is interior, add the ring to current polygon.
                        var area = SignedArea(ring);
                        if (area > 0)
                        {
                            if (isPolygonStarted)
                            {
                                handler.OnEndPolygon();
                            }
                            handler.OnBeginPolygon();
                            isPolygonStarted = true;
                        }
                        handler.OnBeginLinearRing();
                        foreach (var point in ring)
                        {
                            handler.OnPoint(point);
                        }
                        handler.OnEndLinearRing();
                        ring.Clear();
                    }
                    handler.OnEndPolygon();
                    break;

                case PbfGeomType.Unknown:
                    return false;
            }
            return true;
        }

        private float SignedArea(List<Point> ring)
        {
            var area = 0f;
            var prev = ring.LastOrDefault();
            foreach (var curr in ring)
            {
                area += curr.X * prev.Y - curr.Y * prev.X;
                prev = curr;
            }
            return 0.5f * area;
        }
    }
}