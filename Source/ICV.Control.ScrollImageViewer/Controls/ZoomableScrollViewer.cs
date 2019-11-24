using ImageComparisonViewer.Common.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Xaml.Behaviors;
using ICV.Control.ScrollImageViewer.Behaviors;
using System.Collections.Generic;
using System.Reactive.Disposables;
using ImageComparisonViewer.Common.Wpf;
using ICV.Control.ScrollImageViewer.ViewModels;
using ICV.Control.ScrollImageViewer.Extensions;

namespace ICV.Control.ScrollImageViewer.Controls
{
    class ZoomableScrollViewer : ScrollViewer, IDisposable
    {
        private static readonly Type SelfType = typeof(ZoomableScrollViewer);

        protected readonly CompositeDisposable CompositeDisposable = new CompositeDisposable();

        // Controls
        private ScrollContentPresenter _scrollContentPresenter = default!;
        private readonly Grid _contentGrid;
        private readonly Image _mainImage;

        #region ZoomPayloadProperty(TwoWay)

        /// <summary>画像ズーム倍率通知(TwoWay)</summary>
        public ImageZoomPayload ZoomPayload
        {
            get => (ImageZoomPayload)GetValue(ZoomPayloadProperty);
            set => SetValue(ZoomPayloadProperty, value);
        }
        private static readonly DependencyProperty ZoomPayloadProperty =
            DependencyProperty.Register(nameof(ZoomPayload), typeof(ImageZoomPayload), SelfType,
                new FrameworkPropertyMetadata(default(ImageZoomPayload),
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                    | FrameworkPropertyMetadataOptions.AffectsArrange
                    | FrameworkPropertyMetadataOptions.AffectsRender
                    | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((ZoomableScrollViewer)d).OnZoomPayloadChanged((ImageZoomPayload)(e.NewValue))));

        private void OnZoomPayloadChanged(ImageZoomPayload imageZoom)
        {
            // スクロールバーの表示を切り替える
            void UpdateScrollBarVisibility(in ImageZoomPayload zoomMag)
            {
                var visible = ScrollBarVisibility.Hidden;

                // ズームインならスクロールバーを表示
                if (!zoomMag.IsEntire)
                {
                    if (GetEntireZoomMagRatio() < zoomMag.MagRatio)
                        visible = ScrollBarVisibility.Visible;
                }

                this.HorizontalScrollBarVisibility = this.VerticalScrollBarVisibility = visible;
            }

            // 画像サイズの更新前にスクロールバーの表示を更新
            UpdateScrollBarVisibility(imageZoom);

            var image = _mainImage;
            if (!imageZoom.IsEntire)
            {
                // ズーム表示に切り替え
                var imageSourceSize = image.GetImageSourcePixelSize();
                image.Width = imageSourceSize.Width * imageZoom.MagRatio;
                image.Height = imageSourceSize.Height * imageZoom.MagRatio;
            }
            else
            {
                // 全画面表示に切り替え
                var size = GetEntireZoomImageSize();
                image.Width = size.Width;
                image.Height = size.Height;
            }
        }

        #endregion

        #region ScrollOffsetCenterRatioPayloadProperty(TwoWay)

        /// <summary>スクロールバー中央の絶対位置の割合(0~1)(TwoWay)</summary>
        public Point ScrollOffsetCenterRatioPayload
        {
            get => (Point)GetValue(ScrollOffsetCenterRatioPayloadProperty);
            set => SetValue(ScrollOffsetCenterRatioPayloadProperty, value);
        }
        private static readonly DependencyProperty ScrollOffsetCenterRatioPayloadProperty =
            DependencyProperty.Register(nameof(ScrollOffsetCenterRatioPayload), typeof(Point), SelfType,
                new FrameworkPropertyMetadata(default(Point),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((ZoomableScrollViewer)d).OnScrollOffsetCenterRatioPayloadChanged((Point)(e.NewValue))));

        private void OnScrollOffsetCenterRatioPayloadChanged(Point offsetCenterRatio)
        {
            Image_ScrollToCenter(offsetCenterRatio);
        }

        private void Image_ScrollByVectorRatio(Vector vectorRatio)
        {
            Image_ScrollToCenter(ScrollOffsetCenterRatioPayload + vectorRatio);
        }

        private void Image_ScrollByVectorActualSize(Vector vector)
        {
            var imageViewSize = _mainImage.GetControlActualSize();
            var vecRatio = new Vector(vector.X / imageViewSize.Width, vector.Y / imageViewSize.Height);
            Image_ScrollByVectorRatio(-vecRatio);
        }

        private void Image_ScrollToCenter(Point offsetCenterRatio)
        {
            static double clip(double value, double min, double max) => (value <= min) ? min : ((value >= max) ? max : value);

            var scrollViewer = this;
            var contentSize = _scrollContentPresenter.GetControlActualSize();
            var imageSize = _mainImage.GetControlActualSize();

            // 好き勝手に要求された位置を範囲制限する
            var rateRange = GetScrollOffsetRateRange(scrollViewer);
            var newOffset = new Point(
                clip(offsetCenterRatio.X, rateRange.widthMin, rateRange.widthMax),
                clip(offsetCenterRatio.Y, rateRange.heightMin, rateRange.heightMax));

            var contentHalfSize = new Size(contentSize.Width / 2.0, contentSize.Height / 2.0);
            //if (!sviewHalf.IsValidValue()) return;

            var horiOffset = Math.Max(0.0, newOffset.X * imageSize.Width - contentHalfSize.Width);
            var vertOffset = Math.Max(0.0, newOffset.Y * imageSize.Height - contentHalfSize.Height);

            scrollViewer.ScrollToHorizontalOffset(horiOffset);
            scrollViewer.ScrollToVerticalOffset(vertOffset);

            // ズーム倍率管理プロパティの更新
            ScrollOffsetCenterRatioPayload = newOffset;
        }

        // スクロールバー位置の範囲(割合)を取得
        private static (double widthMin, double widthMax, double heightMin, double heightMax)
            GetScrollOffsetRateRange(ScrollViewer sView)
        {
            (double, double, double, double) nolimit = (0.0, 1.0, 0.0, 1.0);

            // 全体表示ならオフセットに制限なし
            if (sView.ExtentWidth < sView.ViewportWidth || sView.ExtentHeight < sView.ViewportHeight)
            {
                return nolimit;
            }
            //else if (sView.ExtentWidth.IsValidValue() && sView.ExtentHeight.IsValidValue())
            else if (sView.ExtentWidth != 0 && sView.ExtentHeight != 0)
            {
                var widthRateMin = (sView.ViewportWidth / 2.0) / sView.ExtentWidth;
                var widthRateMax = (sView.ExtentWidth - sView.ViewportWidth / 2.0) / sView.ExtentWidth;
                var heightRateMin = (sView.ViewportHeight / 2.0) / sView.ExtentHeight;
                var heightRateMax = (sView.ExtentHeight - sView.ViewportHeight / 2.0) / sView.ExtentHeight;
                return (widthRateMin, widthRateMax, heightRateMin, heightRateMax);
            }
            return nolimit;
        }

        #endregion

        #region ImageSourceProperty(OneWay/Inherits)

        /// <summary>ソース画像は親から継承する</summary>
        public BitmapSource ImageSource
        {
            get => (BitmapSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
        private static readonly DependencyProperty ImageSourceProperty =
            BasePanel.ImageSourceProperty.AddOwner(SelfType,
                new FrameworkPropertyMetadata(default,
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                    | FrameworkPropertyMetadataOptions.AffectsRender
                    | FrameworkPropertyMetadataOptions.Inherits,
                    (d, e) => ((ZoomableScrollViewer)d).OnImageSourceChanged((BitmapSource)(e.NewValue))));

        private void OnImageSourceChanged(BitmapSource bitmapSource)
        {
            _mainImage.Source = bitmapSource;
        }

        /// <summary>元画像の読込み中フラグ</summary>
        public bool IsLoadingImage
        {
            get => (bool)GetValue(IsLoadingImageProperty);
            set => SetValue(IsLoadingImageProperty, value);
        }
        private static readonly DependencyProperty IsLoadingImageProperty =
            BasePanel.IsLoadingImageProperty.AddOwner(SelfType,
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.Inherits));

        #endregion

        #region ImageCursorPointProperty(OneWayToSource)

        /// <summary>View画像上のカーソル位置(画像Pixel座標系)</summary>
        public Point ImageCursorPoint
        {
            get => (Point)GetValue(ImageCursorPointProperty);
            set => SetValue(ImageCursorPointProperty, value);
        }
        private static readonly DependencyProperty ImageCursorPointProperty =
            DependencyProperty.Register(nameof(ImageCursorPoint), typeof(Point), SelfType);

        #endregion

        public ZoomableScrollViewer()
        {
            /*
             * [] ControlのリストをGridに詰め込んで、Contentにしたい
             * [x] ViewModelから倍率と表示位置を要求する
             * [x] ViewModelに倍率と表示位置を通知する
             * [x] マウスホイールでズーム倍率を変更する
             * [x] マウスドラッグで表示位置を変更する
             * [x] ViewModelにカーソル位置を通知する
             * [] ズーム変更時に表示中央にズームする
             * [] ダブルクリックでズーム倍率を変更する
             * [] シングルクリックでズーム倍率を一時的に変更する
             * [] ViewModelからサンプリング枠の表示を切り替えたい
             * [] ViewModelにサンプリング枠の位置を通知したい
             */

            this.Loaded += (sender, __) =>
            {
                if (((DependencyObject)sender).TryGetChildControl<ScrollContentPresenter>(out var presenter))
                {
                    presenter.SizeChanged += ScrollContentPresenter_SizeChanged;

                    presenter.MouseLeftDragVectorAsObservable()
                        .Subscribe(vec => Image_ScrollByVectorActualSize(vec))
                        .AddTo(CompositeDisposable);

                    _scrollContentPresenter = presenter;
                }
            };

            var selfBehaviors = Interaction.GetBehaviors(this);
            selfBehaviors.Add(new MouseHorizontalShiftBehavior());
            this.PreviewMouseWheel += ZoomableScrollViewer_PreviewMouseWheel;

            _mainImage = CreateMainImageControl();
            _mainImage.SizeChanged += MainImage_SizeChanged;

            _contentGrid = CreateContentGridControl(_mainImage);
            this.Content = _contentGrid;
        }

        private void ScrollContentPresenter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 全画面表示中にスクロールバー削除なのでサイズが変化したらズーム倍率を更新する
            if (ZoomPayload.IsEntire)
            {
                var entireRatio = GetEntireZoomMagRatio();
                ZoomPayload = new ImageZoomPayload(true, entireRatio);
            }
        }

        private void ZoomableScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                mouseWheelZoom(isZoomIn: e.Delta > 0);

                // 最大ズームでホイールすると画像の表示エリアが移動しちゃうので止める
                e.Handled = true;
            }

            void mouseWheelZoom(bool isZoomIn)
            {
                // ズーム適用前
                var oldImageZoom = ZoomPayload;
                double oldZoomMagRatio = GetCurrentImageZoomMagRatio();

                // ズーム適用
                var newImageZoom = ImageZoomPayload.ZoomMagnification(oldZoomMagRatio, isZoomIn);

                // 全画面表示時を跨ぐ場合は全画面表示にする
                // ◆厳密に比較しすぎて、マウスズーム変更時に似たようなズーム位置が連続するので、もっと雑に比較したほうが良い
                var enrireZoomMag = GetEntireZoomMagRatio();
                if ((oldImageZoom.MagRatio < enrireZoomMag && enrireZoomMag < newImageZoom.MagRatio)
                    || (newImageZoom.MagRatio < enrireZoomMag && enrireZoomMag < oldImageZoom.MagRatio))
                {
                    newImageZoom = new ImageZoomPayload(true, enrireZoomMag);
                }
                ZoomPayload = newImageZoom;
            }
        }

        private void MainImage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var actualSize = e.NewSize;
            if (actualSize.Width == 0 || actualSize.Height == 0) return;

            var sourceSize = ((Image)sender).GetImageSourcePixelSize();
            var magRatio = GetZoomMagRatio(actualSize, sourceSize);
            ZoomPayload = new ImageZoomPayload(ZoomPayload.IsEntire, magRatio);
        }

        #region ZoomSize/Ratio
        /// <summary>画像を全画面表示する場合のサイズを取得</summary>
        /// <returns>全画面表示サイズ</returns>
        private Size GetEntireZoomImageSize()
        {
            var contentSize = _scrollContentPresenter.GetControlActualSize();
            var imageSize = _mainImage.GetImageSourcePixelSize();

            if (imageSize.Height == 0) return default;
            var imageRatio = imageSize.Width / imageSize.Height;

            if (contentSize.Height == 0) return default;
            var contentRatio = contentSize.Width / contentSize.Height;

            double width, height;
            if (imageRatio > contentRatio)
            {
                width = contentSize.Width;      // 横パンパン
                height = contentSize.Width / imageRatio;
            }
            else
            {
                width = contentSize.Height * imageRatio;
                height = contentSize.Height;    // 縦パンパン
            }
            return new Size(width, height);
        }

        /// <summary>全画面表示になるのズーム倍率を返す</summary>
        /// <returns>ズーム倍率(1=100%=等倍)、計算不可時はNaN</returns>
        private double GetEntireZoomMagRatio()
        {
            // 画像サイズ0(x画面切り替え時はNaN
            var imageSize = _mainImage.GetImageSourcePixelSize();
            if (imageSize.Width == 0 || imageSize.Height == 0)
                return double.NaN;

            var entireZoomImageSize = GetEntireZoomImageSize();
            return GetZoomMagRatio(entireZoomImageSize, imageSize);
        }

        /// <summary>表示中画像のズーム倍率を返す</summary>
        /// <returns>ズーム倍率(1=100%=等倍)、計算不可時はNaN</returns>
        private double GetCurrentImageZoomMagRatio()
        {
            // 画像サイズ0(x画面切り替え時はNaN
            var imagePixelSize = _mainImage.GetImageSourcePixelSize();
            if (imagePixelSize.Width == 0 || imagePixelSize.Height == 0)
                return double.NaN;

            var imageViewSize = _mainImage.GetControlActualSize();
            return GetZoomMagRatio(imageViewSize, imagePixelSize);
        }

        // 引数サイズのズーム倍率を求める
        private static double GetZoomMagRatio(in Size newSize, in Size baseSize)
        {
            if (baseSize.Width == 0) throw new DivideByZeroException(nameof(Width));
            if (baseSize.Height == 0) throw new DivideByZeroException(nameof(Height));
            return Math.Min(newSize.Width / baseSize.Width, newSize.Height / baseSize.Height);
        }
        #endregion

        #region CreateControls
        /// <summary>主画像</summary>
        private Image CreateMainImageControl()
        {
            var image = new Image
            {
                UseLayoutRounding = true    // ◆ズーム処理が無限ループで終わらないの暫定対策
            };

            var behaviors = Interaction.GetBehaviors(image);
            behaviors.Add(new ImageBitmapScalingBehavior());

            // Imageコントロール上のマウス位置を実画像の座標系を取得
            image.MouseMoveCursorOnImageAsObservable()
                .Subscribe(point => ImageCursorPoint = point)
                .AddTo(CompositeDisposable);

            return image;
        }

        /// <summary>コンテントに設定されるGridで内部にImageを持つ</summary>
        private static Grid CreateContentGridControl(Image image)
        {
            var grid = new Grid();
            grid.Children.Add(image);
            return grid;
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

                    Interaction.GetBehaviors(this).Detach();
                    foreach (var child in this.GetDescendants())
                    {
                        if (child is DependencyObject depObj)
                            Interaction.GetBehaviors(depObj).Detach();
                    }

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
