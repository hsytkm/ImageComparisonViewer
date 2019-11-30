using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace ICV.Control.ScrollImageViewer.Extensions
{
    static class MouseLongPushObservableExtension
    {
        public enum MouseLongPush
        {
            Start, End
        };

        /// <summary>
        /// クリック長押しイベント
        /// </summary>
        /// <param name="control">対象コントロール</param>
        /// <returns>クリック位置</returns>
        public static IObservable<(MouseLongPush Push, Point Point)> MouseLeftLongPushAsObservable(this UIElement control, bool handled = false)
        {
            var pushMsec = 300d;

            var mouseDown = control.MouseLeftButtonDownAsObservable(handled)
                .Select(e => e.GetPosition((IInputElement)control));
            var mouseUp = control.MouseLeftButtonUpAsObservable(handled)
                .Select(e => e.GetPosition((IInputElement)control));

            var push = mouseDown
                .Delay(TimeSpan.FromMilliseconds(pushMsec))     // 長押し判定
                .TakeUntil(mouseUp)                             // ちょん離しを弾く
                .Repeat()
                .Select(point => (Push: MouseLongPush.Start, Point: point));

            // Start -> Endの順なら発火する
            var release = push.Merge(mouseUp.Select(point => (Push: MouseLongPush.End, Point: point)))
                .Pairwise()
                .Where(x => x.OldItem.Push == MouseLongPush.Start && x.NewItem.Push == MouseLongPush.End)
                .Select(x => x.NewItem);

            return push.Merge(release);
        }

    }
}
