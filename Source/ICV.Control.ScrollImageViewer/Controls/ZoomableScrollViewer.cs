using ICV.Control.ScrollImageViewer.Behaviors;
using ICV.Control.ScrollImageViewer.Extensions;
using ICV.Control.ScrollImageViewer.ViewModels;
using ImageComparisonViewer.Common.Wpf;
using Microsoft.Xaml.Behaviors;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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
        public ImageZoomMag ZoomPayload
        {
            get => (ImageZoomMag)GetValue(ZoomPayloadProperty);
            set => SetValue(ZoomPayloadProperty, value);
        }
        private static readonly DependencyProperty ZoomPayloadProperty =
            DependencyProperty.Register(nameof(ZoomPayload), typeof(ImageZoomMag), SelfType,
                new FrameworkPropertyMetadata(default(ImageZoomMag),
                    FrameworkPropertyMetadataOptions.AffectsMeasure
                    | FrameworkPropertyMetadataOptions.AffectsArrange
                    | FrameworkPropertyMetadataOptions.AffectsRender
                    | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((ZoomableScrollViewer)d).OnZoomPayloadChanged((ImageZoomMag)(e.NewValue))));

        private void OnZoomPayloadChanged(ImageZoomMag imageZoom)
        {
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

            //image.UpdateLayout();
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
            static double clip(double value, double min, double max) => (value <= min) ? min : ((value >= max) ? max : value);

            var scrollViewer = this;
            var contentSize = _scrollContentPresenter.GetControlActualSize();
            var imageSize = _mainImage.GetControlActualSize();

            if (contentSize.Width == 0 || contentSize.Height == 0) return;

            // 好き勝手に要求された位置を範囲制限する
            var rateRange = scrollViewer.GetScrollOffsetRateRange();
            var newOffset = new Point(
                clip(offsetCenterRatio.X, rateRange.widthMin, rateRange.widthMax),
                clip(offsetCenterRatio.Y, rateRange.heightMin, rateRange.heightMax));

            var contentHalfSize = new Size(contentSize.Width / 2.0, contentSize.Height / 2.0);
            //if (!sviewHalf.IsValidValue()) return;

            var horiOffset = newOffset.X * imageSize.Width - contentHalfSize.Width;
            scrollViewer.ScrollToHorizontalOffsetWithLimit(horiOffset);

            var vertOffset = newOffset.Y * imageSize.Height - contentHalfSize.Height;
            scrollViewer.ScrollToVerticalOffsetWithLimit(vertOffset);
        }

        #endregion

        #region ScrollVectorRatioPayload(OneWayToSource)

        /// <summary>スクロール移動量の割合(0~1)(OneWayToSource)</summary>
        public Vector ScrollVectorRatioPayload
        {
            get => (Vector)GetValue(ScrollVectorRatioPayloadProperty);
            set => SetValue(ScrollVectorRatioPayloadProperty, value);
        }
        private static readonly DependencyProperty ScrollVectorRatioPayloadProperty =
            DependencyProperty.Register(nameof(ScrollVectorRatioPayload), typeof(Vector), SelfType);

        private void SetImageOffsetVector(in Point oldPoint, in Point newPoint)
            => ScrollVectorRatioPayload = newPoint - oldPoint;

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
                    | FrameworkPropertyMetadataOptions.AffectsParentArrange
                    | FrameworkPropertyMetadataOptions.AffectsParentMeasure
                    | FrameworkPropertyMetadataOptions.Inherits,
                    (d, e) => ((ZoomableScrollViewer)d).OnImageSourceChanged((BitmapSource)(e.NewValue))));

        private void OnImageSourceChanged(BitmapSource bitmapSource)
        {
            _mainImage.Source = bitmapSource;

            // 画像変化時の表示更新
            OnZoomPayloadChanged(ZoomPayload);
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

        #region ImageViewportProperty(OneWayToSource)

        /// <summary>画像の表示領域</summary>
        public ScrollViewerViewport ImageViewport
        {
            get => (ScrollViewerViewport)GetValue(ImageViewportProperty);
            set => SetValue(ImageViewportProperty, value);
        }
        private static readonly DependencyProperty ImageViewportProperty =
            DependencyProperty.Register(nameof(ImageViewport), typeof(ScrollViewerViewport), SelfType,
                new FrameworkPropertyMetadata(default(ScrollViewerViewport)));

        #endregion

        public ZoomableScrollViewer()
        {
            // Visibleは必要
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;

            /*
             * [] ControlのリストをGridに詰め込んで、Contentにしたい
             * [x] ViewModelから倍率と表示位置を要求する
             * [x] ViewModelに倍率と表示位置を通知する
             * [x] マウスホイールでズーム倍率を変更する
             * [x] マウスドラッグで表示位置を変更する
             * [x] スクロールバー操作で表示位置を更新する
             * [x] ViewModelにカーソル位置を通知する
             * [x] ズーム変更時に表示中央にズームする
             * [x] ダブルクリックでズーム倍率を変更する
             * [x] シングルクリックでズーム倍率を一時的に変更する
             * [] ViewModelからサンプリング枠の表示を切り替えたい
             * [] ViewModelにサンプリング枠の位置を通知したい
             */

            this.Loaded += (sender, __) =>
            {
                var scrollViewer = (ScrollViewer)sender;

                // 表示領域の更新
                ImageViewport = new ScrollViewerViewport(scrollViewer);

                if (scrollViewer.TryGetChildControl<ScrollContentPresenter>(out var presenter))
                {
                    presenter.Loaded += Presenter_Loaded;
                    presenter.SizeChanged += ScrollContentPresenter_SizeChanged;

                    // drag viewport shift
                    presenter.MouseLeftDragVectorAsObservable()
                        .ObserveOnUIDispatcher()
                        .Where(_ => IsZoomingIn(ZoomPayload))   // ズーム中以外は必要ない
                        .Select(vec => -vec)
                        .Subscribe(vec =>
                        {
                            var imageViewSize = _mainImage.GetControlActualSize();
                            ScrollVectorRatioPayload = new Vector(vec.X / imageViewSize.Width, vec.Y / imageViewSize.Height);
                        })
                        .AddTo(CompositeDisposable);

                    // double click zoom
                    presenter.MouseDoubleClickAsObservable()
                        .ObserveOnUIDispatcher()
                        .Subscribe(point => SwitchClickZoomMag(point))
                        .AddTo(CompositeDisposable);

                    // single click zoom
                    bool longPushZooming = false;
                    var mouseLongPushState = presenter.MouseLeftLongPushAsObservable()
                        .Publish()
                        .RefCount();

                    mouseLongPushState
                        .Where(x => x.Push == MouseLongPushObservableExtension.MouseLongPush.Start)
                        .ObserveOnUIDispatcher()
                        .Where(x => ZoomPayload.IsEntire)
                        .Do(_ => longPushZooming = true)
                        .Subscribe(x => SwitchClickZoomMag(x.Point))
                        .AddTo(CompositeDisposable);

                    mouseLongPushState
                        .Where(x => x.Push == MouseLongPushObservableExtension.MouseLongPush.End)
                        .ObserveOnUIDispatcher()
                        .Where(_ => longPushZooming)
                        .Do(_ => longPushZooming = false)
                        .Subscribe(x => SwitchClickZoomMag(x.Point))
                        .AddTo(CompositeDisposable);

                    _scrollContentPresenter = presenter;
                }

                if (scrollViewer.TryGetChildControlFromName<ScrollBar>("PART_VerticalScrollBar", out var scrollBarVert))
                {
                    scrollBarVert.IsVisibleChanged += (_, e) =>
                    {
                        var isVisible = (bool)e.NewValue;
                        var shift = (isVisible ? scrollBarVert.Width : -scrollBarVert.Width) / 2.0;
                        this.ScrollToHorizontalOffsetShiftWithLimit(shift);
                    };
                }

                if (scrollViewer.TryGetChildControlFromName<ScrollBar>("PART_HorizontalScrollBar", out var scrollBarHori))
                {
                    scrollBarHori.IsVisibleChanged += (_, e) =>
                    {
                        var isVisible = (bool)e.NewValue;
                        var shift = (isVisible ? scrollBarHori.Height : -scrollBarHori.Height) / 2.0;
                        this.ScrollToVerticalOffsetShiftWithLimit(shift);
                    };
                }
            };

            this.ScrollChanged += ScrollViewer_ScrollChanged;

            var selfBehaviors = Interaction.GetBehaviors(this);
            selfBehaviors.Add(new MouseHorizontalShiftBehavior());
            this.PreviewMouseWheel += ZoomableScrollViewer_PreviewMouseWheel;

            _mainImage = CreateMainImageControl();
            _mainImage.SizeChanged += MainImage_SizeChanged_UpdateZoomPayload;
            _mainImage.SizeChanged += MainImage_SizeChanged_UpdateScrollBarOffset;

            _contentGrid = CreateContentGridControl(_mainImage);
            this.Content = _contentGrid;
        }

        private void Presenter_Loaded(object sender, RoutedEventArgs e)
        {
            // 全体表示中のタブ切り替えで画像表示
            if (ZoomPayload.IsEntire)
            {
                OnZoomPayloadChanged(ZoomPayload);
            }
            else
            {
                // 表示サイズが伸びた分だけ補正する(Tab切り替え時用)
                var size = ((FrameworkElement)sender).GetControlActualSize();
                this.ScrollToHorizontalOffsetShiftWithLimit(-size.Width / 2.0);
                this.ScrollToVerticalOffsetShiftWithLimit(-size.Height / 2.0);
            }
        }

        private void ScrollContentPresenter_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 全画面表示中にスクロールバー削除などでサイズが変化したらズーム倍率を更新する
            if (ZoomPayload.IsEntire)
            {
                var entireRatio = GetEntireZoomMagRatio();
                ZoomPayload = new ImageZoomMag(true, entireRatio);
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

                // ズーム適用後の倍率
                var newImageZoom = ImageZoomMag.ZoomMagnification(oldZoomMagRatio, isZoomIn);

                // 全体表示時の倍率を取得
                var enrireZoomMag = GetEntireZoomMagRatio();

                // 全画面表示時を跨ぐ場合は全画面表示にする
                if ((oldImageZoom.MagRatio < enrireZoomMag && enrireZoomMag < newImageZoom.MagRatio)
                    || (newImageZoom.MagRatio < enrireZoomMag && enrireZoomMag < oldImageZoom.MagRatio))
                {
                    // 厳密に比較しすぎてズーム変更時にほぼ同じズーム位置が連続するので雑に判定
                    // 4なら%表示の小数点第二位までチェック
                    if (oldImageZoom.MagRatio.AreClose(enrireZoomMag, tolerancePoint: 4)
                        || newImageZoom.MagRatio.AreClose(enrireZoomMag, tolerancePoint: 4))
                    {
                        // 現状が "ほぼ" 全体表示なので跨ぐことはない
                    }
                    else
                    {
                        newImageZoom = new ImageZoomMag(true, enrireZoomMag);
                    }
                }
                ZoomPayload = newImageZoom;
            }
        }

        /// <summary>サイズ変更時にズーム倍率を設定</summary>
        private void MainImage_SizeChanged_UpdateZoomPayload(object sender, SizeChangedEventArgs e)
        {
            var actualSize = e.NewSize;
            if (actualSize.Width == 0 || actualSize.Height == 0) return;

            var sourceSize = _mainImage.GetImageSourcePixelSize();
            var magRatio = GetZoomMagRatio(actualSize, sourceSize);
            ZoomPayload = new ImageZoomMag(ZoomPayload.IsEntire, magRatio);
        }

        /// <summary>サイズ変更時にViewのスクロールバー位置を更新</summary>
        private void MainImage_SizeChanged_UpdateScrollBarOffset(object sender, SizeChangedEventArgs e)
        {
            static double clip(double value, double min, double max) => (value <= min) ? min : ((value >= max) ? max : value);

            var imageViewSize = e.NewSize;
            var contentSize = _scrollContentPresenter.GetControlActualSize();
            var centerRatio = ScrollOffsetCenterRatioPayload;

            // 好き勝手に要求された位置を範囲制限する
            var rateRange = this.GetScrollOffsetRateRange();
            var newOffset = new Point(
                clip(centerRatio.X, rateRange.widthMin, rateRange.widthMax),
                clip(centerRatio.Y, rateRange.heightMin, rateRange.heightMax));

            var contentSizeHalf = new Size(contentSize.Width / 2.0, contentSize.Height / 2.0);
            //if (!contentSizeHalf.IsValidValue()) return;

            var horiOffset = newOffset.X * imageViewSize.Width - contentSizeHalf.Width;
            this.ScrollToHorizontalOffsetWithLimit(horiOffset);

            var vertOffset = newOffset.Y * imageViewSize.Height - contentSizeHalf.Height;
            this.ScrollToVerticalOffsetWithLimit(vertOffset);
        }

        /// <summary>スクロールバー操作時の表示位置の更新</summary>
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var imageViewActualSize = _mainImage.GetControlActualSize();
            //if (!imageViewActualSize.IsValidValue()) return;

            // 表示サイズによる全体判定
            var isEntire = !(e.ViewportWidth < e.ExtentWidth || e.ViewportHeight < e.ExtentHeight);

            // スクロールバーによる全体判定
            //var isEntire = this.HorizontalScrollBarVisibility == ScrollBarVisibility.Hidden
            //    && this.VerticalScrollBarVisibility == ScrollBarVisibility.Hidden;

            Point newCenterRatio;

            // 全体表示なら中央位置を上書き
            if (isEntire)
            {
                newCenterRatio = new Point(0.5, 0.5);
            }
            else
            {
                // 表示が収まる場合は中央(0.5)
                var pointX = (e.ViewportWidth >= imageViewActualSize.Width) ? 0.5
                    : (e.HorizontalOffset + (e.ViewportWidth / 2.0)) / imageViewActualSize.Width;
                var pointY = (e.ViewportHeight >= imageViewActualSize.Height) ? 0.5
                    : (e.VerticalOffset + (e.ViewportHeight / 2.0)) / imageViewActualSize.Height;

                newCenterRatio = new Point(pointX, pointY);
            }

            // スクロールバーの表示切替
            UpdateScrollBarVisibility(ZoomPayload);

            // 表示領域の更新
            ImageViewport = new ScrollViewerViewport(e);
        }

        /// <summary>スクロールバーの表示を切り替える</summary>
        private void UpdateScrollBarVisibility(in ImageZoomMag zoomMag)
        {
            var horiVisible = ScrollBarVisibility.Hidden;
            var vertVisible = ScrollBarVisibility.Hidden;

            // ズームインならスクロールバーを表示
            if (IsZoomingIn(zoomMag))
            {
                var imageSize = _mainImage.GetControlActualSize();
                var contentSize = _scrollContentPresenter.GetControlActualSize();

                if (contentSize.Width < imageSize.Width)
                    horiVisible = ScrollBarVisibility.Visible;

                if (contentSize.Height < imageSize.Height)
                    vertVisible = ScrollBarVisibility.Visible;
            }

            this.HorizontalScrollBarVisibility = horiVisible;
            if (horiVisible == ScrollBarVisibility.Hidden)
                this.ScrollToHorizontalOffsetWithLimit(0);

            this.VerticalScrollBarVisibility = vertVisible;
            if (vertVisible == ScrollBarVisibility.Hidden)
                this.ScrollToVerticalOffsetWithLimit(0);
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

        /// <summary>ズームイン中(全体orズームアウトでない)</summary>
        private bool IsZoomingIn(in ImageZoomMag zoom) =>
            !zoom.IsEntire && GetEntireZoomMagRatio() < zoom.MagRatio;

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

        #region SwitchClickZoomMag

        // クリックズームの状態を切り替える(全画面⇔ズーム)
        private void SwitchClickZoomMag(Point clickPoint)
        {
            if (!ZoomPayload.IsEntire)
            {
                // ここで倍率詰めるのは無理(コントロールサイズが変わっていないため)
                ZoomPayload = ImageZoomMag.Entire; // ToAll
            }
            else
            {
                ZoomPayload = ImageZoomMag.MagX1;  // ToZoom

                var scrollContentActualSize = _scrollContentPresenter.GetControlActualSize();
                var imageViewActualSize = _mainImage.GetControlActualSize();

                // ズーム表示への切り替えならスクロールバーを移動(ImageViewSizeを変更した後に実施する)
                //if (imageViewActualSize.IsValidValue())
                {
                    static double clip(double value, double min, double max) => (value <= min) ? min : ((value >= max) ? max : value);

                    // 親ScrollViewerから子Imageまでのサイズ
                    var imageControlSizeOffset = new Size(
                        Math.Max(0, scrollContentActualSize.Width - imageViewActualSize.Width) / 2d,
                        Math.Max(0, scrollContentActualSize.Height - imageViewActualSize.Height) / 2d);

                    // 子Image基準のマウス位置
                    var mousePos = new Point(
                        Math.Max(0, clickPoint.X - imageControlSizeOffset.Width),
                        Math.Max(0, clickPoint.Y - imageControlSizeOffset.Height));

                    // ズーム後の中心座標の割合
                    var newPoint = new Point(
                        clip(mousePos.X / imageViewActualSize.Width, 0, 1),
                        clip(mousePos.Y / imageViewActualSize.Height, 0, 1));

                    SetImageOffsetVector(oldPoint: ScrollOffsetCenterRatioPayload, newPoint);
                }
            }
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
