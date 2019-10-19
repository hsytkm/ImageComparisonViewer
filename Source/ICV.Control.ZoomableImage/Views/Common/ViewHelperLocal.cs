using System;
using System.Windows;

namespace ICV.Control.ZoomableImage.Views.Common
{
    static class ViewHelperLocal
    {
        public static Size GetControlActualSize(this FrameworkElement fe) =>
            (fe is null) ? default : new Size(fe.ActualWidth, fe.ActualHeight);

        public static bool IsValidValue(this double d)
        {
            if (double.IsNaN(d)) return false;
            if (d == 0.0) return false;
            return true;
        }

        public static bool IsValidValue(this in Size size)
        {
            if (!size.Width.IsValidValue()) return false;
            if (!size.Height.IsValidValue()) return false;
            return true;
        }

        public static Rect Round(this Rect rect)
        {
            rect.X = Math.Round(rect.X);
            rect.Y = Math.Round(rect.Y);
            rect.Width = Math.Round(rect.Width);
            rect.Height = Math.Round(rect.Height);
            return rect;
        }
        
        public static Rect MinLength(this Rect rect, double minLength)
        {
            if (minLength <= 0.0) throw new ArgumentOutOfRangeException(nameof(minLength));

            if (rect.Width < minLength) rect.Width = minLength;
            if (rect.Height < minLength) rect.Height = minLength;
            return rect;
        }

    }
}
