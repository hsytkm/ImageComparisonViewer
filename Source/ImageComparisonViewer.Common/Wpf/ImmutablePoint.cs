using System;
using System.Windows;

namespace ImageComparisonViewer.Common.Wpf
{
    // http://proprogrammer.hatenadiary.jp/entry/2018/08/18/172739
    public readonly struct ImmutablePoint
    {
        public readonly double X;
        public readonly double Y;

        public ImmutablePoint(double x, double y)
            => (X, Y) = (x, y);

        public static implicit operator ImmutablePoint(Point source)
            => new ImmutablePoint(source.X, source.Y);

        public static implicit operator Point(in ImmutablePoint source)
            => new Point(source.X, source.Y);

        public static ImmutablePoint operator +(in ImmutablePoint point, in ImmutableVector vector)
            => new ImmutablePoint(point.X + vector.X, point.Y + vector.Y);

        //public static bool operator ==(in ImmutablePoint point1, in ImmutablePoint point2)
        //    => (point1.X == point2.X && point1.Y == point2.Y);

        //public static bool operator !=(in ImmutablePoint point1, in ImmutablePoint point2)
        //    => !(point1 == point2);

        public override string ToString() => $"({X:f4}, {Y:f4})";
    }
}
