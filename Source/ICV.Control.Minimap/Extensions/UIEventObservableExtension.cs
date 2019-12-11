using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace ICV.Control.Minimap.Extensions
{
    static class UIEventObservableExtension
    {
        public static IObservable<SizeChangedEventArgs> SizeChangedAsObservable(this FrameworkElement control, bool handled = false)
            => Observable.FromEvent<SizeChangedEventHandler, SizeChangedEventArgs>
            (
                handler => (sender, e) => { e.Handled = handled; handler(e); },
                handler => control.SizeChanged += handler,
                handler => control.SizeChanged -= handler
            );

        //public static IObservable<DataTransferEventArgs> TargetUpdatedAsObservable(this FrameworkElement control, bool handled = false)
        //    => Observable.FromEvent<EventHandler<DataTransferEventArgs>, DataTransferEventArgs>
        //    (
        //        handler => (sender, e) => { e.Handled = handled; handler(e); },
        //        handler => control.TargetUpdated += handler,
        //        handler => control.TargetUpdated -= handler
        //    );

        //public static IObservable<ScrollChangedEventArgs> ScrollChangedAsObservable(this ScrollViewer control, bool handled = false)
        //    => Observable.FromEvent<ScrollChangedEventHandler, ScrollChangedEventArgs>
        //    (
        //        handler => (sender, e) => { e.Handled = handled; handler(e); },
        //        handler => control.ScrollChanged += handler,
        //        handler => control.ScrollChanged -= handler
        //    );

        public static IObservable<DragDeltaEventArgs> DragDeltaAsObservable(this Thumb control, bool handled = false)
            => Observable.FromEvent<DragDeltaEventHandler, DragDeltaEventArgs>
            (
                handler => (sender, e) => { e.Handled = handled; handler(e); },
                handler => control.DragDelta += handler,
                handler => control.DragDelta -= handler
            );

    }
}
