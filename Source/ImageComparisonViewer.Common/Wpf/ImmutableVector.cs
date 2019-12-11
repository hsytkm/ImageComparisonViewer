using System;
using System.Windows;

namespace ImageComparisonViewer.Common.Wpf
{
    // http://proprogrammer.hatenadiary.jp/entry/2018/08/18/172739
    public readonly struct ImmutableVector
    {
        public readonly double X;
        public readonly double Y;

        public ImmutableVector(double x, double y)
            => (X, Y) = (x, y);

        public static implicit operator ImmutableVector(Vector source)
            => new ImmutableVector(source.X, source.Y);

        public static implicit operator Vector(in ImmutableVector source)
            => new Vector(source.X, source.Y);

    }
}