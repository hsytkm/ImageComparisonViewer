using System;
using System.Windows.Controls;

namespace ImageComparisonViewer.Common.Wpf
{
    public readonly struct ScrollViewerViewport
    {
        // ScrollViewer内のサイズ
        public readonly double ExtentWidth;
        public readonly double ExtentHeight;

        // ScrollBar位置
        public readonly double HorizontalOffset;
        public readonly double VerticalOffset;

        // ScrollViewerの実際にViewに表示されているサイズ
        public readonly double ViewportWidth;
        public readonly double ViewportHeight;

        public ScrollViewerViewport(ScrollViewer viewer)
        {
            ExtentWidth = viewer.ExtentWidth;
            ExtentHeight = viewer.ExtentHeight;
            HorizontalOffset = viewer.HorizontalOffset;
            VerticalOffset = viewer.VerticalOffset;
            ViewportWidth = viewer.ViewportWidth;
            ViewportHeight = viewer.ViewportHeight;
        }

        public ScrollViewerViewport(ScrollChangedEventArgs e)
        {
            ExtentWidth = e.ExtentWidth;
            ExtentHeight = e.ExtentHeight;
            HorizontalOffset = e.HorizontalOffset;
            VerticalOffset = e.VerticalOffset;
            ViewportWidth = e.ViewportWidth;
            ViewportHeight = e.ViewportHeight;
        }

        public override string ToString()
            => $"Extent({ExtentWidth:f2}, {ExtentHeight:f2}), " +
            $"Offset({HorizontalOffset:f2}, {VerticalOffset:f2}), " +
            $"Viewport({ViewportWidth:f2}, {ViewportHeight:f2})";

    }
}
