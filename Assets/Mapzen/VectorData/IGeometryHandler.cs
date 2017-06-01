using System;

namespace Mapzen.VectorData
{
    public interface IGeometryHandler
    {
        bool OnPoint(Point point);

        bool OnBeginLineString();

        bool OnEndLineString();

        bool OnBeginLinearRing();

        bool OnEndLinearRing();

        bool OnBeginPolygon();

        bool OnEndPolygon();
    }
}
