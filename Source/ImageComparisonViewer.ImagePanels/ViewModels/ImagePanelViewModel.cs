using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core;
using Prism.Ioc;
using Prism.Mvvm;
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
        private readonly ImageSources _imageSources = default!;
        public int ContentIndex { get; }

        /// <summary>
        /// Viewのアクティブ状態(非アクティブなら購読を停止)
        /// </summary>
        public ReactiveProperty<bool> IsActive { get; } = new ReactiveProperty<bool>();

        /// <summary>
        /// ディレクトリPATH(未選択ならnull)
        /// </summary>
        public ReactiveProperty<string?> DirectoryPath { get; }

        /// <summary>
        /// 画像Drop時のイベント通知
        /// </summary>
        public ReactiveProperty<IReadOnlyList<string>> DropEvent { get; } =
            new ReactiveProperty<IReadOnlyList<string>>(mode: ReactivePropertyMode.None);

        /// <summary>
        /// 表示画像リスト
        /// </summary>
        public ReadOnlyReactiveProperty<IReadOnlyList<string>> SourceImagesPath { get; }

        /// <summary>
        /// 選択中の画像PATH(未選択ならnull)
        /// </summary>
        public ReactiveProperty<string?> SelectedImagePath { get; }

        public ImagePanelViewModel(IContainerExtension container, int contentIndex)
        {
            ContentIndex = contentIndex;

            _imageSources = container.Resolve<ImageSources>();

            // VM→M
            DropEvent
                .Subscribe(paths => _imageSources.SetDroppedPaths(contentIndex, paths))
                .AddTo(CompositeDisposable);

            #region DirectoryPath

            // TwoWay
            DirectoryPath = _imageSources.ImageDirectries[contentIndex]
                .ToReactivePropertyAsSynchronized(x => x.DirectoryPath, mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);

            // 対象画像リストの読み出し(◆拡張性の判定が不十分)
            // M→VMの通知はIsActiveを参照して非表示時は無視する(◆より良い実装あれば変えたい)
            SourceImagesPath = DirectoryPath
                .CombineLatest(IsActive, (directoryPath, isActive) => (directoryPath, isActive))
                //.Do(x => Debug.WriteLine($"Log: {contentIndex}, {x.isActive}, {x.directoryPath}"))
                .Where(x => x.isActive).Select(x => x.directoryPath)
                .Select(x => Directory.EnumerateFiles(x, "*.jpg", SearchOption.TopDirectoryOnly).ToList())
                .Cast<IReadOnlyList<string>>()
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);

            #endregion

            #region SelectedFilePath

            // TwoWay
            // M→VMの通知はIsActiveを参照して非表示時は無視する(◆より良い実装あれば変えたい)
            SelectedImagePath = _imageSources.ImageDirectries[contentIndex]
                .ToReactivePropertyAsSynchronized(x => x.SelectedFilePath, mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            #endregion

        }

    }
}
