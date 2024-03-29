﻿using Reactive.Bindings.Extensions;
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

            var mouseDown = control.MouseLeftButtonDownEventAsObservable(handled)
                .Select(e => e.GetPosition((IInputElement)control))
                .Do(_ => control.CaptureMouse())
                .Publish()
                .RefCount();

            var mouseUp = control.MouseLeftButtonUpEventAsObservable(handled)
                .Select(e => e.GetPosition((IInputElement)control))
                .Do(_ => control.ReleaseMouseCapture())
                .Publish()
                .RefCount();
            
            var push = mouseDown
                .Delay(TimeSpan.FromMilliseconds(pushMsec))     // 長押し判定
                .TakeUntil(mouseUp)                             // ちょん離しを弾く
                .Repeat()
                .Select(point => (Push: MouseLongPush.Start, Point: point))
                .Publish()
                .RefCount();

            // Start -> Endの順なら発火する
            var release = push
                .SelectMany(mouseUp.Select(point => (Push: MouseLongPush.End, Point: point)).Take(1))
                .Publish()
                .RefCount();

            return push.Merge(release);
        }

    }
}
