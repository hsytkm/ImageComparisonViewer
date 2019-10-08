using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ICV.Control.ExplorerAddressBar.Converters
{
    [ValueConversion(typeof(Visibility), typeof(Visibility))]
    class InverseVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility v && v == Visibility.Visible)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            throw new NotImplementedException();
    }
}
