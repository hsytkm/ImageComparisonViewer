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

        public ReactiveProperty<Vector> ScrollVectorRatio { get; } =
            new ReactiveProperty<Vector>(mode: ReactivePropertyMode.None);

        public ReactiveProperty<Point> ImageScrollOffsetCenterRatio { get; } =
            new ReactiveProperty<Point>(mode: ReactivePropertyMode.DistinctUntilChanged);

        public ReadOnlyReactiveProperty<ScrollViewerViewport> ImageViewport { get; } = default!;

        public MinimapViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            var compositeDirectory = container.Resolve<ICompositeImageDirectory>();
            var imageDirectory = compositeDirectory.ImageDirectries[parameter.ContentIndex];

            var imageSource = imageDirectory
                .ObserveProperty(x => x.SelectedImage)
                .Publish()
                .RefCount();

            ImageSource = imageSource
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            ImageSourceSize = imageSource
                .Select(x => (x is null) ? default : new Size(x.Width, x.Height))
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            ScrollVectorRatio
                .Subscribe(v => compositeDirectory.SetImageShiftRatio(parameter.ContentIndex, v))
                .AddTo(CompositeDisposable);

            ImageViewport = imageDirectory
                .ObserveProperty(x => x.ImageViewport)
                //.Do(x => Debug.WriteLine($"{parameter.ContentIndex}: {x}"))
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
