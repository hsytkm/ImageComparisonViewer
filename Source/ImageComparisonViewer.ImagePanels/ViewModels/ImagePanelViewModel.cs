using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core;
using Prism.Ioc;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace ImageComparisonViewer.ImagePanels.ViewModels
{
    class ImagePanelViewModel : DisposableBindableBase
    {
        private readonly ImageSources _imageSources = default!;

        public int ContentIndex
        {
            get => _contentIndex;
            set => SetProperty(ref _contentIndex, value);
        }
        private int _contentIndex;

        /// <summary>
        /// Viewのアクティブ状態(非アクティブなら購読を停止)
        /// </summary>
        public ReactiveProperty<bool> IsActive { get; } = new ReactiveProperty<bool>();

        /// <summary>
        /// ディレクトリPATH(DistinctUntilChangedでないとVM⇔Mで変更通知地獄に陥る)
        /// </summary>
        public ReactiveProperty<string> DirectoryPath { get; } =
            new ReactiveProperty<string>(mode: ReactivePropertyMode.DistinctUntilChanged);

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
        /// 選択中の画像PATH
        /// </summary>
        public ReactiveProperty<string> SelectedImagePath { get; } = new ReactiveProperty<string>();

        public ImagePanelViewModel(IContainerExtension container)
        {
            _imageSources = container.Resolve<ImageSources>();

            // VM->M
            DropEvent
                .Subscribe(paths =>
                {
                    _imageSources.SetDroppedPaths(ContentIndex, paths);

                    // ファイルドロップ直後の表示画像選択
                    if (paths.Any()) SelectedImagePath.Value = GetFilePath(paths[0]);
                })
                .AddTo(CompositeDisposable);

            // VM->M
            DirectoryPath
                .Where(x => Directory.Exists(x))  //念のためチェック
                .Subscribe(path => _imageSources.SetDirectryPath(ContentIndex, path))
                .AddTo(CompositeDisposable);

            // VM<-M
            // Listener系はIsActiveを参照して非表示時は無視するようにしています(◆より良い実装あれば変えたい)
            var sourceDirectoryCache = _imageSources.DirectriesPath
                .CollectionChangedAsObservable()
                .Where(e => e.Action == NotifyCollectionChangedAction.Replace)
                .Where(e => e.NewStartingIndex == ContentIndex)
                //.Do(e => Debug.WriteLine($"Log1: {ContentIndex}, {IsActive.Value}, {e.NewStartingIndex}, {e.NewItems.Cast<string>().First()}"))
                .Select(e => e.NewItems.Cast<string>().First())
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);
            var sourceDirectory = sourceDirectoryCache
                .CombineLatest(IsActive, (path, isActive) => (path, isActive))
                //.Do(x => Debug.WriteLine($"Log2: {ContentIndex}, {x.isActive}, {x.path}"))
                .Where(x => x.isActive).Select(x => x.path)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);

            // Model画像ディレクトリ変化時の処理
            sourceDirectory
                .Subscribe(x => DirectoryPath.Value = x)
                .AddTo(CompositeDisposable);

            // 対象画像リストの読み出し(◆拡張性の判定が不十分)
            SourceImagesPath = sourceDirectory
                .Select(x => Directory.EnumerateFiles(x, "*.jpg", SearchOption.TopDirectoryOnly).ToList())
                .Cast<IReadOnlyList<string>>()
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

        }

        /// <summary>
        /// ファイルPATHを取得する
        /// </summary>
        /// <param name="droppedPath"></param>
        /// <returns></returns>
        private static string GetFilePath(string droppedPath)
        {
            if (File.Exists(droppedPath))
                return droppedPath;

            // ディレクトリなら先頭ファイル
            if (Directory.Exists(droppedPath))
                return Directory.EnumerateFiles(droppedPath, "*", SearchOption.TopDirectoryOnly).First();

            throw new FileNotFoundException(droppedPath);
        }

    }
}
