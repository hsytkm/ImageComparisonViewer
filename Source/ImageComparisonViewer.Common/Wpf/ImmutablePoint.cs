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

    }
}
