using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Common.Wpf;
using ImageComparisonViewer.Core.Images;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ICV.Control.Minimap
{
    class MinimapViewModel : DisposableBindableBase
    {
        public ReadOnlyReactiveProperty<BitmapSource?> ImageSource { get; } = default!;
        public ReadOnlyReactiveProperty<Size> ImageSourceSize { get; } = default!;

        public ReadOnlyReactiveProperty<bool> IsVisible { get; } = default!;

        public ReactiveProperty<Vector> ScrollVectorRatio { get; } =
            new ReactiveProperty<Vector>(mode: ReactivePropertyMode.None);

        public ReactiveProperty<Point> ImageScrollOffsetCenterRatio { get; } =
            new ReactiveProperty<Point>(mode: ReactivePropertyMode.DistinctUntilChanged);

        public ReadOnlyReactiveProperty<ScrollViewerViewport> ImageViewport { get; } = default!;

        public MinimapViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            var compositeDirectory = container.Resolve<ICompositeImageDirectory>();
            var imageDirectory = compositeDirectory.ImageDirectries[parameter.ContentIndex];

            var imageSourceObservable = imageDirectory
                .ObserveProperty(x => x.SelectedImage)
                .Publish()
                .RefCount();

            ImageSource = imageSourceObservable
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            ImageSourceSize = imageSourceObservable
                .Select(x => (x is null) ? default : new Size(x.Width, x.Height))
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            var isZoomingIn = imageDirectory
                .ObserveProperty(x => x.IsZoomingIn)
                //.Do(b => Debug.WriteLine($"isZoomingIn({parameter.ContentIndex}): {b}"))
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            // 画像が拡大表示中ならミニマップを表示
            IsVisible = ImageSource
                .CombineLatest(isZoomingIn, (ImageSource, IsZoomingIn) => (ImageSource, IsZoomingIn))
                .Select(x => (x.ImageSource != null) && x.IsZoomingIn)
                //.Do(b => Debug.WriteLine($"IsVisible({parameter.ContentIndex}): {b}"))
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            // 全ての画像ディレクトリに変更を通知する
            ScrollVectorRatio
                .Subscribe(v => compositeDirectory.SetImageShiftRatioToAll(parameter.ContentIndex, v))
                .AddTo(CompositeDisposable);

            // 主画像の表示範囲
            ImageViewport = imageDirectory
                .ObserveProperty(x => x.ImageViewport)
                //.Do(x => Debug.WriteLine($"ImageViewport({parameter.ContentIndex}): {x}"))
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            //// FromM
            //imageDirectory
            //    .ObserveProperty(x => x.OffsetCenterRatio)
            //    .Subscribe(point => ImageScrollOffsetCenterRatio.Value = point)
            //    .AddTo(CompositeDisposable);

            //// ToM
            //ImageScrollOffsetCenterRatio
            //    .Subscribe(point => compositeDirectory.SetImageOffsetCentergRatio(parameter.ContentIndex, point))
            //    .AddTo(CompositeDisposable);

        }

    }
}
