using System;
using System.Reactive.Linq;
using System.Windows;

namespace ICV.Control.ScrollImageViewer.Extensions
{
    static class FrameworkElementCommonObservableExtension
    {
        //public static IObservable<RoutedEventArgs> LoadedEventAsObservable(this FrameworkElement control, bool handled = false)
        //    => Observable.FromEvent<RoutedEventHandler, RoutedEventArgs>
        //    (
        //        handler => (sender, e) => { e.Handled = handled; handler(e); },
        //        handler => control.Loaded += handler,
        //        handler => control.Loaded -= handler
        //    );

        //public static IObservable<SizeChangedEventArgs> SizeChangedEventAsObservable(this FrameworkElement control, bool handled = false)
        //    => Observable.FromEvent<SizeChangedEventHandler, SizeChangedEventArgs>
        //    (
        //        handler => (sender, e) => { e.Handled = handled; handler(e); },
        //        handler => control.SizeChanged += handler,
        //        handler => control.SizeChanged -= handler
        //    );

    }
}
