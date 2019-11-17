using System;
using System.Collections.Generic;
using System.Windows;

namespace ImageComparisonViewer.Common.Wpf
{
    public static class WpfViewHelper
    {
        public static Size GetControlActualSize(this FrameworkElement fe) =>
            (fe is null) ? default : new Size(fe.ActualWidth, fe.ActualHeight);


    }
}
