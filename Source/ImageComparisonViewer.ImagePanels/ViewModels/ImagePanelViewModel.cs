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
        /// ディレクトリPATH(未選択ならnull)
        /// </summary>
        public ReactiveProperty<string?> DirectoryPath
        {
            get => _directoryPath;
            private set => SetProperty(ref _directoryPath, value);
        }
        private ReactiveProperty<string?> _directoryPath = default!;

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
        /// 表示画像リスト
        /// </summary>
        public ReadOnlyReactiveProperty<IReadOnlyList<string>> SourceImagesPath { get; private set; } = default!;

        /// <summary>
        /// 選択中の画像PATH(未選択ならnull)
        /// </summary>
        public ReactiveProperty<string?> SelectedImagePath
        {
            get => _selectedImagePath;
            private set => SetProperty(ref _selectedImagePath, value);
        }
        private ReactiveProperty<string?> _selectedImagePath = default!;

        private string _contentMessage;

        public ImagePanelViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            _container = container;
            ContentIndex = parameter.ContentIndex;
            ContentCount = parameter.ContentCount;
            _contentMessage = $"{ContentIndex}/{ContentCount}";


            var imageSources = _container.Resolve<ImageSources>();

            DropEvent = new ReactiveProperty<IReadOnlyList<string>>(mode: ReactivePropertyMode.None).AddTo(CompositeDisposable);

            // VM→M
            DropEvent
                .Subscribe(paths => imageSources.SetDroppedPaths(ContentIndex, paths))
                .AddTo(CompositeDisposable);

            #region DirectoryPath

            // TwoWay
            DirectoryPath = imageSources.ImageDirectries[ContentIndex]
                .ToReactivePropertyAsSynchronized(x => x.DirectoryPath, mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);

            DirectoryPath
                .Subscribe(x => Debug.WriteLine($"DirectoryPath({_contentMessage}): {x}"))
                .AddTo(CompositeDisposable);

            // 対象画像リストの読み出し(◆拡張性の判定が不十分)
            SourceImagesPath = DirectoryPath
                .Select(x =>
                {
                    if (x is null) return Enumerable.Empty<string>().ToList();
                    return Directory.EnumerateFiles(x, "*.jpg", SearchOption.TopDirectoryOnly).ToList();
                })
                .Cast<IReadOnlyList<string>>()
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);

            #endregion

            #region SelectedFilePath

            // TwoWay
            SelectedImagePath = imageSources.ImageDirectries[ContentIndex]
                .ToReactivePropertyAsSynchronized(x => x.SelectedFilePath, mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            #endregion

        }

    }
}
