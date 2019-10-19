using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ICV.Control.ZoomableImage.Views
{
    class ZoomableImageGrid : Grid
    {
        private static readonly Type SelfType = typeof(ZoomableImageGrid);

        #region ImageSourceProperty(OneWay/Inherits)

        // 表示する元画像
        internal static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(BitmapSource), SelfType,
                new FrameworkPropertyMetadata(default,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));

        public BitmapSource ImageSource
        {
            get => (BitmapSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        // 元画像の読込み中フラグ
        internal static readonly DependencyProperty IsLoadingImageProperty =
            DependencyProperty.Register(nameof(IsLoadingImage), typeof(bool), SelfType,
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.Inherits));

        public bool IsLoadingImage
        {
            get => (bool)GetValue(IsLoadingImageProperty);
            set => SetValue(IsLoadingImageProperty, value);
        }

        #endregion

    }
}
