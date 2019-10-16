using ImageComparisonViewer.Common.Mvvm;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ICV.Control.ZoomableImage.Views.Behaviors
{
    class ImageBitmapScalingBehavior : BehaviorBase<Image>
    {
        protected override void OnLoaded()
        {
            base.OnLoaded();
            AssociatedObject.SizeChanged += AssociatedObject_SizeChanged;
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            AssociatedObject.SizeChanged -= AssociatedObject_SizeChanged;
        }

        /// <summary>
        /// 画像コントロールのサイズ変更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void AssociatedObject_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!(sender is Image image)) return;
            UpdateBitmapScalingMode(image);
        }

        // レンダリングオプションの指定(100%以上の拡大ズームならPixelが見える設定にする)
        private static void UpdateBitmapScalingMode(Image image)
        {
            if (!(image.Source is BitmapSource bitmap)) return;

            var mode = (image.ActualWidth < bitmap.PixelWidth) || (image.ActualHeight < bitmap.PixelHeight)
                ? BitmapScalingMode.HighQuality : BitmapScalingMode.NearestNeighbor;

            RenderOptions.SetBitmapScalingMode(image, mode);
        }

    }
}
