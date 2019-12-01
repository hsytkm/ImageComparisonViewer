using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;

namespace ICV.Control.ScrollImageViewer.Extensions
{
    static class MouseCommonObservableExtension
    {
        //public static IObservable<MouseEventArgs> PreviewMouseLeftButtonUpEventAsObsAsObservable(this UIElement control, bool handled = false)
        //    => Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>
        //    (
        //        handler => (sender, e) => { e.Handled = handled; handler(e); },
        //        handler => control.PreviewMouseLeftButtonUp += handler,
        //        handler => control.PreviewMouseLeftButtonUp -= handler
        //    );

        public static IObservable<MouseEventArgs> MouseLeftButtonDownEventAsObservable(this UIElement control, bool handled = false)
            => Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>
            (
                handler => (sender, e) => { e.Handled = handled; handler(e); },
                handler => control.MouseLeftButtonDown += handler,
                handler => control.MouseLeftButtonDown -= handler
            );

        public static IObservable<MouseEventArgs> MouseLeftButtonUpEventAsObservable(this UIElement control, bool handled = false)
            => Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>
            (
                handler => (sender, e) => { e.Handled = handled; handler(e); },
                handler => control.MouseLeftButtonUp += handler,
                handler => control.MouseLeftButtonUp -= handler
            );

        //public static IObservable<MouseEventArgs> MouseLeaveEventAsObsAsObservable(this UIElement control, bool handled = false)
        //    => Observable.FromEvent<MouseEventHandler, MouseEventArgs>
        //    (
        //        handler => (sender, e) => { e.Handled = handled; handler(e); },
        //        handler => control.MouseLeave += handler,
        //        handler => control.MouseLeave -= handler
        //    );

        public static IObservable<MouseEventArgs> MouseMoveEventAsObsAsObservable(this UIElement control, bool handled = false)
            => Observable.FromEvent<MouseEventHandler, MouseEventArgs>
            (
                handler => (sender, e) => { e.Handled = handled; handler(e); },
                handler => control.MouseMove += handler,
                handler => control.MouseMove -= handler
            );

        //public static IObservable<MouseWheelEventArgs> PreviewMouseWheelEventAsObsAsObservable(this UIElement control, bool handled = false)
        //    => Observable.FromEvent<MouseWheelEventHandler, MouseWheelEventArgs>
        //    (
        //        handler => (sender, e) => { e.Handled = handled; handler(e); },
        //        handler => control.PreviewMouseWheel += handler,
        //        handler => control.PreviewMouseWheel -= handler
        //    );

    }
}
