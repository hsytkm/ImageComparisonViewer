using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace ICV.Control.ScrollImageViewer.Extensions
{
    static class MouseDoubleClickObservableExtension
    {
        private static IObservable<MouseEventArgs> MouseLeftButtonDownAsObservable(this UIElement control, bool handled = false)
            => Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>
            (
                handler => (sender, e) => { e.Handled = handled; handler(e); },
                handler => control.MouseLeftButtonDown += handler,
                handler => control.MouseLeftButtonDown -= handler
            );

        /// <summary>
        /// ダブルクリックイベント
        /// </summary>
        /// <param name="control">対象コントロール</param>
        /// <returns>クリック位置</returns>
        public static IObservable<Point> MouseDoubleClickAsObservable(this UIElement control, bool handled = false)
        {
            var zipSpanMsec = 200d;
            var moveDistanceMax = 3d;

            var clickPoint = control.MouseLeftButtonDownAsObservable(handled)
                .Select(e => e.GetPosition((IInputElement)control));

            // Slide P.86 https://www.slideshare.net/torisoup/unity-unirx
            // ThrottleでなくSpanがポイント(レスポンス改善)
            return clickPoint.Buffer(clickPoint.Delay(TimeSpan.FromMilliseconds(zipSpanMsec)))
                .Where(x => x.Count >= 2)
                .Select(x => (FirstPoint: x.First(), LastPoint: x.Last()))
                // 高速に画像シフトするとダブルクリック判定されるのでマウス位置が動いていないことをチェック
                .Where(x => x.FirstPoint.GetDistance(x.LastPoint) <= moveDistanceMax)
                .Select(x => x.FirstPoint);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetDistance(this Point p1, Point p2)
        {
            return Math.Sqrt((p2.X - p1.X) * (p2.X - p1.X) + (p2.Y - p1.Y) * (p2.Y - p1.Y));
        }
    }
}
