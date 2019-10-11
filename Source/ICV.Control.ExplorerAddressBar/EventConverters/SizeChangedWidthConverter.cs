using Reactive.Bindings.Interactivity;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace ICV.Control.ExplorerAddressBar.EventConverters
{
    /// <summary>
    /// SizeChangedイベントで水平サイズを返す
    /// </summary>
    class SizeChangedWidthConverter : ReactiveConverter<dynamic, double>
    {
        protected override IObservable<double> OnConvert(IObservable<dynamic> source)
        {
            return source
                .Cast<SizeChangedEventArgs>()
                .Select(e => e.NewSize.Width);
        }
    }

    /// <summary>
    /// Loadedイベントで水平サイズを返す
    /// </summary>
    class LoadedWidthConverter : ReactiveConverter<dynamic, double>
    {
        protected override IObservable<double> OnConvert(IObservable<dynamic> source)
        {
            return source
                .Select(_ => (AssociateObject is FrameworkElement fe) ? fe.ActualWidth : 0);
        }
    }
}
