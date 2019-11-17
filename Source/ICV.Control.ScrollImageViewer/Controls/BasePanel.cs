using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ICV.Control.ScrollImageViewer.Controls
{
    class BasePanel : Grid
    {
        private static readonly Type SelfType = typeof(BasePanel);

        /// <summary>表示する元画像</summary>
        public BitmapSource ImageSource
        {
            get => (BitmapSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
        internal static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(BitmapSource), SelfType,
                new FrameworkPropertyMetadata(default,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));


        /// <summary>元画像の読込み中フラグ</summary>
        public bool IsLoadingImage
        {
            get => (bool)GetValue(IsLoadingImageProperty);
            set => SetValue(IsLoadingImageProperty, value);
        }
        internal static readonly DependencyProperty IsLoadingImageProperty =
            DependencyProperty.Register(nameof(IsLoadingImage), typeof(bool), SelfType,
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.Inherits));

    }
}
