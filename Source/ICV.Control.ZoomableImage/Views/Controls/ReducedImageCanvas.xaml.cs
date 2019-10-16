using ICV.Control.ZoomableImage.Views.Common;
using ImageComparisonViewer.Common.Mvvm;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ICV.Control.ZoomableImage.Views.Controls
{
    /// <summary>
    /// ReducedImageCanvas.xaml の相互作用ロジック
    /// </summary>
    public partial class ReducedImageCanvas : DisposableUserControl
    {
        private static readonly Type SelfType = typeof(ReducedImageCanvas);

        #region ScrollOffsetVectorRatioRequestProperty(OneWayToSource)

        private static readonly DependencyProperty ScrollOffsetVectorRatioRequestProperty =
            DependencyProperty.Register(
                nameof(ScrollOffsetVectorRatioRequest),
                typeof(Vector),
                SelfType);

        public Vector ScrollOffsetVectorRatioRequest
        {
            get => (Vector)GetValue(ScrollOffsetVectorRatioRequestProperty);
            set => SetValue(ScrollOffsetVectorRatioRequestProperty, value);
        }

        #endregion

        public ReducedImageCanvas()
        {
            InitializeComponent();

            // 縮小画像のドラッグ操作を主画像に伝える
            ThumbViewport.DragDeltaAsObservable()
                .Subscribe(e =>
                {
                    var thumbImageActualSize = ViewHelperLocal.GetControlActualSize(ThumbImage);
                    if (!thumbImageActualSize.IsValidValue()) return;

                    // スクロール位置の変化割合を通知
                    ScrollOffsetVectorRatioRequest = new Vector(
                        e.HorizontalChange / thumbImageActualSize.Width,
                        e.VerticalChange / thumbImageActualSize.Height);
                })
                .AddTo(CompositeDisposable);

            this.Loaded += (_, __) =>
            {
                if (this.Parent.TryGetChildControl<ScrollViewer>(out var scrollViewer))
                {
                    // 主画像のスクロール更新時にViewportを更新する
                    scrollViewer.ScrollChangedAsObservable()
                        .Subscribe(UpdateThumbnailViewport)
                        .AddTo(CompositeDisposable);

                    if (scrollViewer.TryGetChildControl<Image>(out var mainImage))
                    {
                        // 主画像の更新時に縮小画像も連動して更新
                        mainImage.TargetUpdatedAsObservable()
                            .Select(e => e.OriginalSource as Image)
                            .Select(image => image?.Source as BitmapSource)
                            .Where(x => x != null)
                            .Subscribe(x => ThumbImage.Source = x)
                            .AddTo(CompositeDisposable);
                    }
                }
            };
        }

        // 主画像のスクロール更新時にViewportを更新する
        private void UpdateThumbnailViewport(ScrollChangedEventArgs e)
        {
            static double clip(double value, double min, double max) => (value <= min) ? min : ((value >= max) ? max : value);

            var thumbViewport = ThumbViewport;

            var thumbImageActualSize = ViewHelperLocal.GetControlActualSize(ThumbImage);
            if (!thumbImageActualSize.IsValidValue()) return;

            // ExtentWidth/Height が ScrollViewer 内の広さ
            // ViewportWidth/Height が ScrollViewer で実際に表示されているサイズ

            if (!e.ExtentWidth.IsValidValue() || !e.ExtentHeight.IsValidValue()) return;
            var xfactor = thumbImageActualSize.Width / e.ExtentWidth;
            var yfactor = thumbImageActualSize.Height / e.ExtentHeight;

            var left = e.HorizontalOffset * xfactor;
            left = clip(left, 0.0, thumbImageActualSize.Width - thumbViewport.MinWidth);

            var top = e.VerticalOffset * yfactor;
            top = clip(top, 0.0, thumbImageActualSize.Height - thumbViewport.MinHeight);

            var width = e.ViewportWidth * xfactor;
            width = clip(width, thumbViewport.MinWidth, thumbImageActualSize.Width);

            var height = e.ViewportHeight * yfactor;
            height = clip(height, thumbViewport.MinHeight, thumbImageActualSize.Height);

            Canvas.SetLeft(thumbViewport, left);
            Canvas.SetTop(thumbViewport, top);
            thumbViewport.Width = width;
            thumbViewport.Height = height;

            CombinedGeometry.Geometry2 = new RectangleGeometry(new Rect(left, top, width, height));
        }

    }
}
