using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Common.Wpf;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace ICV.Control.ScrollImageViewer.Behaviors
{
    class ImageMouseCursorBehavior : BehaviorBase<Image>
    {
        #region ImageCursorPointProperty(OneWayToSource)

        /// <summary>View画像上のカーソル位置(画像Pixel座標系)</summary>
        public Point ImageCursorPoint
        {
            get => (Point)GetValue(ImageCursorPointProperty);
            set => SetValue(ImageCursorPointProperty, value);
        }
        public static readonly DependencyProperty ImageCursorPointProperty =
            DependencyProperty.Register(nameof(ImageCursorPoint), typeof(Point), typeof(ImageMouseCursorBehavior));

        #endregion

        protected override void OnLoaded()
        {
            base.OnLoaded();
            AssociatedObject.MouseMove += AssociatedObject_MouseMove;
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
        }

        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is Image image)) return;
            if (!(image.Source is BitmapSource source)) return;

            var imageViewSize = image.GetControlActualSize();
            var imageSourceSize = new Size(source.PixelWidth, source.PixelHeight);

            // 原画像のピクセル位置を返す
            //if (imageViewSize.IsValidValue() && imageSourceSize.IsValidValue())
            {
                static double clip(double value, double min, double max) => (value <= min) ? min : ((value >= max) ? max : value);

                var cursorPoint = e.GetPosition((IInputElement)image);
                var ox = Math.Floor(cursorPoint.X * imageSourceSize.Width / (imageViewSize.Width - 1));
                var x = clip(ox, 0, imageSourceSize.Width - 1);
                var oy = Math.Floor(cursorPoint.Y * imageSourceSize.Height / (imageViewSize.Height - 1));
                var y = clip(oy, 0, imageSourceSize.Height - 1);

                //Debug.WriteLine($"({imageSourceSize.Width:f2}, {imageSourceSize.Height:f2})  ({imageViewSize.Width:f2}, {imageViewSize.Height:f2})  ({cursorPoint.X:f2}, {cursorPoint.Y:f2})  ({ox}, {oy})  ({x}, {y})");

                ImageCursorPoint = new Point(x, y);
            }
        }

    }
}
