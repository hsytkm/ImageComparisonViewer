using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core.Images;
using Prism.Ioc;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ICV.Control.ScrollImageViewer.ViewModels
{
    class ScrollImageViewerViewModel : DisposableBindableBase
    {
        // 主画像
        public ReadOnlyReactiveProperty<BitmapSource?> ImageSource { get; }
        public ReadOnlyReactiveProperty<bool> IsLoadingImage { get; }

        // 各画像の表示エリアを連動させるかフラグ(false=連動しない)
        public ReadOnlyReactiveProperty<bool> IsImageViewerInterlock { get; }

        // 縮小画像の表示可能フラグ(false=表示禁止)
        public ReadOnlyReactiveProperty<bool> CanVisibleReducedImage { get; }

        // サンプリング枠のレイヤー(true=画像上, false=スクロール上)
        public ReadOnlyReactiveProperty<bool> IsVisibleSamplingFrameOnImage { get; }

        // サンプリング枠のレイヤー(false=画像上, true=スクロール上)
        public ReadOnlyReactiveProperty<bool> IsVisibleSamplingFrameOnScroll { get; }

        // ズーム倍率の管理(TwoWay)
        public ReactiveProperty<ImageZoomMag> ImageZoomMagPayload { get; } =
            new ReactiveProperty<ImageZoomMag>(mode: ReactivePropertyMode.DistinctUntilChanged,
                initialValue: ImageZoomMag.MagX1);

        // スクロールオフセット位置(TwoWay)
        public ReactiveProperty<Point> ImageScrollOffsetCenterRatio { get; } =
            new ReactiveProperty<Point>(mode: ReactivePropertyMode.DistinctUntilChanged,
                initialValue: new Point(0.5, 0.5));

        // 実座標系のサンプリング枠の位置(TwoWay)
        public ReactiveProperty<Rect> SamplingFrameRect { get; } =
            new ReactiveProperty<Rect>(mode: ReactivePropertyMode.DistinctUntilChanged);

        public ReactiveCommand ZoomAllCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ZoomX1Command { get; } = new ReactiveCommand();
        public ReactiveCommand OffsetCenterCommand { get; } = new ReactiveCommand();

        public ReactiveProperty<Point> PointTest { get; } = new ReactiveProperty<Point>();

        public ScrollImageViewerViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            var compositeDirectory = container.Resolve<ICompositeImageDirectory>();
            var imageDirectory = compositeDirectory.ImageDirectries[parameter.ContentIndex];

            #region ImageSource
            ImageSource = imageDirectory.ObserveProperty(x => x.SelectedImage)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);

            IsLoadingImage = ImageSource.Select(x => x != null)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);
            #endregion

            #region Zoom
            imageDirectory
                .ObserveProperty(x => x.ZoomMagRatio)
                .Subscribe(x => ImageZoomMagPayload.Value = double.IsNaN(x)
                                ? ImageZoomMag.Entire : new ImageZoomMag(false, x))
                .AddTo(CompositeDisposable);

            ImageZoomMagPayload
                .Select(x => x.IsEntire ? double.NaN : x.MagRatio)
                .Subscribe(mag => compositeDirectory.SetImageZoomMagRatio(parameter.ContentIndex, mag))
                .AddTo(CompositeDisposable);
            #endregion

            #region Offset
            imageDirectory
                .ObserveProperty(x => x.OffsetCenterRatio)
                .Subscribe(point => ImageScrollOffsetCenterRatio.Value = point)
                .AddTo(CompositeDisposable);

            ImageScrollOffsetCenterRatio
                .Subscribe(point => compositeDirectory.SetImageOffsetCentergRatio(parameter.ContentIndex, point))
                .AddTo(CompositeDisposable);
            #endregion

            var viewSettings = container.Resolve<ViewSettings>();

            IsImageViewerInterlock = viewSettings.ObserveProperty(x => x.IsImageViewerInterlock)
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            CanVisibleReducedImage = viewSettings.ObserveProperty(x => x.CanVisibleReducedImage)
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            IsVisibleSamplingFrameOnImage = new[]
                {
                    IsLoadingImage,
                    viewSettings.ObserveProperty(x => x.IsVisibleImageOverlapSamplingFrame),
                    viewSettings.ObserveProperty(x => x.IsVisibleSamplingFrameOnImage)
                }
                .CombineLatestValuesAreAllTrue()
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            IsVisibleSamplingFrameOnScroll = new[]
                {
                    IsLoadingImage,
                    viewSettings.ObserveProperty(x => x.IsVisibleImageOverlapSamplingFrame),
                    viewSettings.ObserveProperty(x => x.IsVisibleSamplingFrameOnImage).Inverse()
                }
                .CombineLatestValuesAreAllTrue()
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            #region forDebug
            // View通知情報のデバッグ表示
            var indexMessage = $"VM({parameter.ContentIndex}/{parameter.ContentCount})";

            ImageZoomMagPayload
                .Subscribe(x => Debug.WriteLine($"{indexMessage}-ImageZoomPayload: {x.IsEntire} => {(x.MagRatio * 100.0):f4} %"))
                .AddTo(CompositeDisposable);

            ImageScrollOffsetCenterRatio
                .Subscribe(x => Debug.WriteLine($"{indexMessage}-ImageScrollOffsetCenterRatio: ({x.X:f4}, {x.Y:f4})"))
                .AddTo(CompositeDisposable);

            SamplingFrameRect
                .Subscribe(x => Debug.WriteLine($"{indexMessage}-SamplingFrameRect: ({x.X:f2}, {x.Y:f2}) {x.Width:f2} x {x.Height:f2}"))
                .AddTo(CompositeDisposable);

            ZoomAllCommand
                .Subscribe(x =>
                {
                    // 全画面の再要求を行うと、Viewで設定した倍率をクリアしてしまうので行わない
                    if (!ImageZoomMagPayload.Value.IsEntire)
                        ImageZoomMagPayload.Value = new ImageZoomMag(true, double.NaN);
                })
                .AddTo(CompositeDisposable);

            ZoomX1Command
                .Subscribe(x => ImageZoomMagPayload.Value = new ImageZoomMag(false, 1.0))
                .AddTo(CompositeDisposable);

            OffsetCenterCommand
                .Subscribe(x => ImageScrollOffsetCenterRatio.Value = new Point(0.5, 0.5))
                .AddTo(CompositeDisposable);

            //PointTest.Subscribe(x => Debug.WriteLine($"VM-PointTest: {x}")).AddTo(CompositeDisposable);
            #endregion

        }

    }

}
