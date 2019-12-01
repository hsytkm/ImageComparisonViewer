using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ICV.Control.ScrollImageViewer.Extensions
{
    static class ImageCursorObservableExtension
    {
        /// <summary>
        /// Imageコントロール上のマウス位置を実画像の座標系で返す
        /// </summary>
        /// <param name="image"></param>
        /// <param name="handled"></param>
        /// <returns></returns>
        public static IObservable<Point> MouseMoveCursorOnImageAsObservable(this Image image, bool handled = false)
        {
            return image.MouseMoveEventAsObsAsObservable(handled)
                .Select(e => e.GetPosition(image))
                .Select(point => GetCursorPointOnImageSource(
                    point, new Size(image.ActualWidth, image.ActualHeight), image.GetImageSourcePixelSize()));
        }

        /// <summary>
        /// 原画像のピクセル位置を返す
        /// </summary>
        /// <param name="cursorPoint">Viewのカーソル位置</param>
        /// <param name="viewActualSize">ViewのImageコントロールのActualサイズ</param>
        /// <param name="imageSourceSize">Imageコントロールの現画像のサイズ</param>
        /// <returns></returns>
        private static Point GetCursorPointOnImageSource(in Point cursorPoint, in Size viewActualSize, in Size imageSourceSize)
        {
            //if (!imageViewSize.IsValidValue() || !imageSourceSize.IsValidValue()) return default;

            static double clip(double value, double min, double max) => (value <= min) ? min : ((value >= max) ? max : value);

            var ox = Math.Floor(cursorPoint.X * imageSourceSize.Width / (viewActualSize.Width - 1));
            var x = clip(ox, 0, imageSourceSize.Width - 1);
            var oy = Math.Floor(cursorPoint.Y * imageSourceSize.Height / (viewActualSize.Height - 1));
            var y = clip(oy, 0, imageSourceSize.Height - 1);

            //Debug.WriteLine($"({imageSourceSize.Width:f2}, {imageSourceSize.Height:f2})  ({imageViewSize.Width:f2}, {imageViewSize.Height:f2})  ({cursorPoint.X:f2}, {cursorPoint.Y:f2})  ({ox}, {oy})  ({x}, {y})");

            return new Point(x, y);
        }

    }
}
