using System;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace ICV.Control.ScrollImageViewer.Extensions
{
    static class ScrollViewerExtension
    {
        #region ScrollOffsetWithLimit
        /// <summary>
        /// 水平オフセットの補正量を反映する
        /// </summary>
        /// <param name="scrollViewer"></param>
        /// <param name="shift">スクロールオフセット補正量</param>
        public static void ScrollToHorizontalOffsetShiftWithLimit(this ScrollViewer scrollViewer, double shift)
        {
            var offset = scrollViewer.HorizontalOffset + shift;
            var setOffset = offset.LimitOffset(scrollViewer.ScrollableWidth);
            scrollViewer.ScrollToHorizontalOffset(setOffset);
        }

        /// <summary>
        /// 垂直オフセットの補正量を反映する
        /// </summary>
        /// <param name="scrollViewer"></param>
        /// <param name="shift">スクロールオフセット補正量</param>
        public static void ScrollToVerticalOffsetShiftWithLimit(this ScrollViewer scrollViewer, double shift)
        {
            var offset = scrollViewer.VerticalOffset + shift;
            var setOffset = offset.LimitOffset(scrollViewer.ScrollableHeight);
            scrollViewer.ScrollToVerticalOffset(setOffset);
        }

        /// <summary>
        /// 水平オフセット値を制限して設定する
        /// </summary>
        /// <param name="scrollViewer"></param>
        /// <param name="offset">スクロールオフセット絶対値</param>
        public static void ScrollToHorizontalOffsetWithLimit(this ScrollViewer scrollViewer, double offset)
        {
            var setOffset = offset.LimitOffset(scrollViewer.ScrollableWidth);
            scrollViewer.ScrollToHorizontalOffset(setOffset);
        }

        /// <summary>
        /// 垂直オフセット値を制限して設定する
        /// </summary>
        /// <param name="scrollViewer"></param>
        /// <param name="offset">スクロールオフセット絶対値</param>
        public static void ScrollToVerticalOffsetWithLimit(this ScrollViewer scrollViewer, double offset)
        {
            var setOffset = offset.LimitOffset(scrollViewer.ScrollableHeight);
            scrollViewer.ScrollToVerticalOffset(setOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double LimitOffset(this double value, double max)
        {
            var min = 0.0;
            if (value <= min)
            {
                return min;
            }
            // max == 0 なら制限しない(コントロールがまだ更新されていない)
            else if (max != 0 && max <= value)
            {
                return max;
            }
            return value;
        }
        #endregion

        /// <summary>
        /// スクロールバーオフセットの範囲(割合)を取得
        /// </summary>
        /// <param name="viewer"></param>
        /// <returns></returns>
        public static (double widthMin, double widthMax, double heightMin, double heightMax)
            GetScrollOffsetRateRange(this ScrollViewer viewer)
        {
            (double, double, double, double) nolimit = (0.0, 1.0, 0.0, 1.0);

            // 全体表示ならオフセットに制限なし
            if (viewer.ExtentWidth < viewer.ViewportWidth || viewer.ExtentHeight < viewer.ViewportHeight)
            {
                return nolimit;
            }
            //else if (sView.ExtentWidth.IsValidValue() && sView.ExtentHeight.IsValidValue())
            else if (viewer.ExtentWidth != 0 && viewer.ExtentHeight != 0)
            {
                var widthRateMin = (viewer.ViewportWidth / 2.0) / viewer.ExtentWidth;
                var widthRateMax = (viewer.ExtentWidth - viewer.ViewportWidth / 2.0) / viewer.ExtentWidth;
                var heightRateMin = (viewer.ViewportHeight / 2.0) / viewer.ExtentHeight;
                var heightRateMax = (viewer.ExtentHeight - viewer.ViewportHeight / 2.0) / viewer.ExtentHeight;
                return (widthRateMin, widthRateMax, heightRateMin, heightRateMax);
            }
            return nolimit;
        }

    }
}
