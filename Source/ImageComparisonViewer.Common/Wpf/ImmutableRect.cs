using System;
using System.Windows;

namespace ImageComparisonViewer.Common.Wpf
{
    // http://proprogrammer.hatenadiary.jp/entry/2018/08/18/172739
    public readonly struct ImmutableRect
    {
        public readonly double X;
        public readonly double Y;
        public readonly double Width;
        public readonly double Height;

        public ImmutableRect(double x, double y, double width, double height)
            => (X, Y, Width, Height) = (x, y, width, height);

        public static implicit operator ImmutableRect(Rect source)
            => new ImmutableRect(source.X, source.Y, source.Width, source.Height);

        public static implicit operator Rect(in ImmutableRect source)
            => new Rect(source.X, source.Y, source.Width, source.Height);

    }
}
