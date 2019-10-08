using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace ImageComparisonViewer.ImagePanels.ViewModels
{
    class ImagePanelViewModel : DisposableBindableBase
    {
        private readonly IContainerExtension _container;

        public int ContentIndex { get; }
        public int ContentCount { get; }

        /// <summary>
        /// 画像Drop時のイベント通知
        /// </summary>
        public ReactiveProperty<IReadOnlyList<string>> DropEvent
        {
            get => _dropEvent;
            private set => SetProperty(ref _dropEvent, value);
        }
        private ReactiveProperty<IReadOnlyList<string>> _dropEvent = default!;

        /// <summary>
        /// ディレクトリPATH(未選択ならnull)
        /// </summary>
        public ReadOnlyReactiveProperty<string?> DirectoryPath
        {
            get => _directoryPath;
            private set => SetProperty(ref _directoryPath, value);
        }
        private ReadOnlyReactiveProperty<string?> _directoryPath = default!;

        /// <summary>
        /// 選択中の画像PATH(未選択ならnull)
        /// </summary>
        public ReadOnlyReactiveProperty<string?> SelectedImagePath
        {
            get => _selectedImagePath;
            private set => SetProperty(ref _selectedImagePath, value);
        }
        private ReadOnlyReactiveProperty<string?> _selectedImagePath = default!;

        public ImagePanelViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            ContentIndex = parameter.ContentIndex;
            ContentCount = parameter.ContentCount;

            _container = container;
            var imageSources = _container.Resolve<ImageSources>();
            var imageSource = imageSources.ImageDirectries[ContentIndex];

            // ドロップファイル通知
            DropEvent = new ReactiveProperty<IReadOnlyList<string>>(mode: ReactivePropertyMode.None).AddTo(CompositeDisposable);
            DropEvent
                .Subscribe(paths => imageSources.SetDroppedPaths(ContentIndex, paths))
                .AddTo(CompositeDisposable);

            // 読み出しディレクトリ購読
            DirectoryPath = imageSource
                .ObserveProperty(x => x.DirectoryPath)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);

            // 選択ファイル購読
            SelectedImagePath = imageSource
                .ObserveProperty(x => x.SelectedFilePath)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

        }

    }
}
