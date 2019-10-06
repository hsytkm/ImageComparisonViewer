using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace ImageComparisonViewer.Common.Mvvm
{
    public static class ViewHelper
    {
        public static bool TryGetChildControl<T>(DependencyObject d, [NotNullWhen(true)] out T? child)
            where T : DependencyObject
        {
            if (d is T control1)
            {
                child = control1;
                return true;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(d); i++)
            {
                if (TryGetChildControl<T>(VisualTreeHelper.GetChild(d, i), out var control2))
                {
                    child = control2;
                    return true;
                }
            }

            child = default;
            return false;
        }

    }
}
