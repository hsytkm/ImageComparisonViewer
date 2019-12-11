using ICV.Control.Minimap.Extensions;
using ImageComparisonViewer.Common.Wpf;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ICV.Control.Minimap.Controls
{
    class MinimapCanvas : Canvas, IDisposable
    {
        private static readonly Type SelfType = typeof(MinimapCanvas);

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        #region ImageSourceProperty(OneWay)

        /// <summary>縮小画像の元画像</summary>
        public BitmapSource ImageSource
        {
            get => (BitmapSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
        internal static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(BitmapSource), SelfType,
                new FrameworkPropertyMetadata(default,
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                    | FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((MinimapCanvas)d).OnImageSourceChanged((BitmapSource)(e.NewValue))));

        private void OnImageSourceChanged(BitmapSource bitmapSource)
        {
            _miniImage.Source = bitmapSource;

            var visibility = (bitmapSource is null) ? Visibility.Collapsed : Visibility.Visible;
            _thumbViewport.Visibility = visibility;
        }

        #endregion

        #region ScrollVectorRatioRequestProperty(OneWayToSource)

        public Vector ScrollVectorRatioRequest
        {
            get => (Vector)GetValue(ScrollVectorRatioRequestProperty);
            set => SetValue(ScrollVectorRatioRequestProperty, value);
        }

        private static readonly DependencyProperty ScrollVectorRatioRequestProperty =
            DependencyProperty.Register(nameof(ScrollVectorRatioRequest), typeof(Vector), SelfType);

        #endregion

        #region ImageViewportProperty(OneWay)

        public ScrollViewerViewport ImageViewport
        {
            get => (ScrollViewerViewport)GetValue(ImageViewportProperty);
            set => SetValue(ImageViewportProperty, value);
        }

        private static readonly DependencyProperty ImageViewportProperty =
            DependencyProperty.Register(nameof(ImageViewport), typeof(ScrollViewerViewport), SelfType,
                new FrameworkPropertyMetadata(default(ScrollViewerViewport),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((MinimapCanvas)d).OnImageViewportChanged((ScrollViewerViewport)(e.NewValue))));

        // 主画像のスクロール更新時にViewportを更新する
        private void OnImageViewportChanged(in ScrollViewerViewport viewport)
        {
            static double clip(double value, double min, double max) => (value <= min) ? min : ((value >= max) ? max : value);

            var thumbViewport = _thumbViewport;

            var miniImageActualSize = _miniImage.GetControlActualSize();
            if (miniImageActualSize.Width == 0 || miniImageActualSize.Height == 0) return;

            // ExtentWidth/Height が ScrollViewer 内の広さ
            // ViewportWidth/Height が ScrollViewer で実際に表示されているサイズ

            if (viewport.ExtentWidth == 0 || viewport.ExtentHeight == 0) return;
            var xfactor = miniImageActualSize.Width / viewport.ExtentWidth;
            var yfactor = miniImageActualSize.Height / viewport.ExtentHeight;

            var left = viewport.HorizontalOffset * xfactor;
            left = clip(left, 0.0, miniImageActualSize.Width - thumbViewport.MinWidth);

            var top = viewport.VerticalOffset * yfactor;
            top = clip(top, 0.0, miniImageActualSize.Height - thumbViewport.MinHeight);

            var width = viewport.ViewportWidth * xfactor;
            width = clip(width, thumbViewport.MinWidth, miniImageActualSize.Width);

            var height = viewport.ViewportHeight * yfactor;
            height = clip(height, thumbViewport.MinHeight, miniImageActualSize.Height);

            Canvas.SetLeft(thumbViewport, left);
            Canvas.SetTop(thumbViewport, top);
            thumbViewport.Width = width;
            thumbViewport.Height = height;

            _geometryFilter.Geometry2 = new RectangleGeometry(new Rect(left, top, width, height));
        }

        #endregion

        #region ScrollOffsetRatioProperty(TwoWay)

        /// <summary>表示中央位置(割合)</summary>
        //public Point ScrollOffsetRatio
        //{
        //    get => (Point)GetValue(ScrollOffsetRatioProperty);
        //    set => SetValue(ScrollOffsetRatioProperty, value);
        //}

        //private static readonly DependencyProperty ScrollOffsetRatioProperty =
        //    DependencyProperty.Register(nameof(ScrollOffsetRatio), typeof(Point), SelfType,
        //        new FrameworkPropertyMetadata(default,
        //            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        //            (d, e) => ((MinimapCanvas)d).OnScrollOffsetRatioChanged((Point)(e.NewValue))));

        //private void OnScrollOffsetRatioChanged(Point newPoint)
        //{
        //}

        #endregion

        private readonly Image _miniImage = default!;
        private readonly CombinedGeometry _geometryFilter = default!;
        private readonly Path _path = default!;
        private readonly Thumb _thumbViewport = default!;

        public MinimapCanvas()
        {
            // Create child controls
            _miniImage = CreateMinimapImageControl();
            _geometryFilter = CreateCombinedGeometry();
            _path = CreatePath(_geometryFilter);
            _thumbViewport = CreateThumb();

            // 縮小画像のドラッグ操作を主画像に伝える
            _thumbViewport.DragDeltaAsObservable()
                .Subscribe(e =>
                {
                    var imageSize = _miniImage.GetControlActualSize();
                    if (imageSize.Width == 0 || imageSize.Height == 0) return;

                    // スクロール位置の変化割合を通知
                    ScrollVectorRatioRequest = new Vector(
                        e.HorizontalChange / imageSize.Width,
                        e.VerticalChange / imageSize.Height);

                    //Debug.WriteLine($"{ScrollVectorRatioRequest.X}, {ScrollVectorRatioRequest.Y}");
                })
                .AddTo(CompositeDisposable);

            // Canvasのサイズ変更
            _miniImage.SizeChangedAsObservable()
                .Subscribe(e =>
                {
                    this.Width = e.NewSize.Width;
                    this.Height = e.NewSize.Height;
                })
                .AddTo(CompositeDisposable);

            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.Children.Add(_miniImage);
            this.Children.Add(_path);
            this.Children.Add(_thumbViewport);
        }

        #region Create child controls
        /// <Image x:Name="ThumbImage"
        ///        Width="100"
        ///        Stretch="Uniform"
        ///        Source="{Binding ImageSource, ElementName=root, Mode=OneWay}" />
        private static Image CreateMinimapImageControl()
        {
            var image = new Image
            {
                Width = 100.0,
                Stretch = Stretch.Uniform,
            };
            image.SetValue(Canvas.LeftProperty, 0.0);
            image.SetValue(Canvas.TopProperty, 0.0);

            return image;
        }

        /// <Path Fill="#7FFFFFFF" >
        ///     <Path.Data>
        ///         <CombinedGeometry x:Name="CombinedGeometryFilter"
        ///                           GeometryCombineMode="Xor" />
        ///     </Path.Data>
        /// </Path>
        private static Path CreatePath(CombinedGeometry geometry)
        {
            var path = new Path
            {
                Fill = new SolidColorBrush(Color.FromArgb(0x7f, 0xff, 0xff, 0xff)),
                Data = geometry,
            };
            path.SetValue(Canvas.LeftProperty, 0.0);
            path.SetValue(Canvas.TopProperty, 0.0);

            return path;
        }
        private static CombinedGeometry CreateCombinedGeometry()
        {
            var geometry = new CombinedGeometry
            {
                GeometryCombineMode = GeometryCombineMode.Xor,
            };
            return geometry;
        }

        /// <Thumb x:Name="ThumbViewport"
        ///        MinHeight="4"
        ///        MinWidth="4" >
        ///     <Thumb.Template>
        ///         <ControlTemplate TargetType="{x:Type Thumb}">
        ///             <Border BorderBrush="Red"
        ///                     BorderThickness="2"
        ///                     Background="Transparent" />
        ///         </ControlTemplate>
        ///     </Thumb.Template>
        /// </Thumb>
        private static Thumb CreateThumb()
        {
            var thumb = new Thumb
            {
                MinHeight = 4,
                MinWidth = 4,
            };
            thumb.SetValue(Canvas.LeftProperty, 0.0);
            thumb.SetValue(Canvas.TopProperty, 0.0);

            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.BorderBrushProperty, Brushes.Red);
            border.SetValue(Border.BorderThicknessProperty, new Thickness(2));
            border.SetValue(Border.BackgroundProperty, Brushes.Transparent);

            thumb.Template = new ControlTemplate(typeof(Thumb))
            {
                VisualTree = border
            };

            return thumb;
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    /*TODO: マネージ状態を破棄します (マネージ オブジェクト)*/
                    CompositeDisposable.Dispose();
                    this.DisposeDescendants();
                }

                // TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
                // TODO: 大きなフィールドを null に設定します。

                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion

    }
}
