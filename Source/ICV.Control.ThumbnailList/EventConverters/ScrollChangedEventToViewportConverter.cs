using System;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using Reactive.Bindings.Interactivity;

namespace ICV.Control.ThumbnailList.EventConverters
{
    /// <summary>
    /// ScrollChangedイベントで水平方向の表示領域の情報を返す
    /// </summary>
    class ScrollChangedEventToViewportConverter : ReactiveConverter<dynamic, HorizontalScrolltRatio>
    {
        protected override IObservable<HorizontalScrolltRatio> OnConvert(IObservable<dynamic> source)
        {
            return source
                .Cast<ScrollChangedEventArgs>()
                .Select(e => new HorizontalScrolltRatio(e));
        }
    }

    /// <summary>
    /// Loadedイベントで水平方向の表示領域の情報を返す
    /// </summary>
    class LoadedEventToViewportConverter : ReactiveConverter<dynamic, HorizontalScrolltRatio>
    {
        protected override IObservable<HorizontalScrolltRatio> OnConvert(IObservable<dynamic> source)
        {
            return source
                .Select(x => new HorizontalScrolltRatio(this.AssociateObject as ScrollViewer));
        }
    }

    /// <summary>
    /// 水平方向の表示領域の情報
    /// </summary>
    readonly struct HorizontalScrolltRatio
    {
        /// <summary>
        /// 表示範囲の中央の割合(0~1)
        /// </summary>
        public readonly double CenterRatio;

        /// <summary>
        /// 全要素と表示範囲の割合(0~1) 要素が全て表示されていたら1.0
        /// </summary>
        public readonly double ViewportRatio;

        public HorizontalScrolltRatio(ScrollChangedEventArgs e)
        {
            CenterRatio = GetCenterRatio(e.ExtentWidth, e.ViewportWidth, e.HorizontalOffset);
            ViewportRatio = GetViewportRatio(e.ExtentWidth, e.ViewportWidth);
        }

        public HorizontalScrolltRatio(ScrollViewer? scroll)
        {
            if (scroll is null)
            {
                CenterRatio = ViewportRatio = 0;
            }
            else
            {
                CenterRatio = GetCenterRatio(scroll.ExtentWidth, scroll.ViewportWidth, scroll.HorizontalOffset);
                ViewportRatio = GetViewportRatio(scroll.ExtentWidth, scroll.ViewportWidth);
            }
        }

        /// <summary>
        /// 表示範囲の中央の割合
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetCenterRatio(double length, double viewport, double offset) =>
            (length == 0) ? 0 : ClipRatio((offset + (viewport / 2)) / length);

        /// <summary>
        /// 全要素と表示範囲の割合(要素が全て表示されていたら1.0)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double GetViewportRatio(double length, double viewport) =>
            (length == 0) ? 0 : ClipRatio(viewport / length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double ClipRatio(double value) =>
            (value <= 0) ? 0 : (1 < value ? 1 : value);
    }

}
