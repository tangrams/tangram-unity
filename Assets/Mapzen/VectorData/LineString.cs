using System.Collections.Generic;
using System.Linq;

namespace Mapzen.VectorData
{
    public abstract class LineString
    {
        IEnumerable<Point> Points { get; }

        IEnumerable<Point> ClosedPoints
        {
            get
            {
                if (Points.Any())
                {
                    return Points.Concat(Points.Take(1));
                }
                else
                {
                    return Points;
                }
            }
        }
    }
}
