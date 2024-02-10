namespace Mapzen.VectorData
{
    public struct Point
    {
        public Point(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        public float X;
        public float Y;

        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }
    }
}
