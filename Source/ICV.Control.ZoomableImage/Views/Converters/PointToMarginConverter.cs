using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ICV.Control.ZoomableImage.Views.Converters
{
    [ValueConversion(typeof(Point), typeof(Thickness))]
    public class PointToMarginConverter : IValueConverter
    {
        public static readonly Thickness _thicknessZero = new Thickness(0);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Point p)) return _thicknessZero;
            return new Thickness(p.X, p.Y, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
