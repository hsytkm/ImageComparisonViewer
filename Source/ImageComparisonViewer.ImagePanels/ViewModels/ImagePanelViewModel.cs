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
        /// 選択中の画像PATH(未選択ならnull)
        /// </summary>
        public ReactiveProperty<string?> SelectedImagePath { get; } = new ReactiveProperty<string?>(mode: ReactivePropertyMode.DistinctUntilChanged);

        public ImagePanelViewModel(IContainerExtension container)
        {
            _imageSources = container.Resolve<ImageSources>();

            // VM→M
            DropEvent
                .Subscribe(paths => _imageSources.SetDroppedPaths(ContentIndex, paths))
                .AddTo(CompositeDisposable);

            // VM→M
            DirectoryPath
                .Where(x => Directory.Exists(x))  //念のためチェック
                .Subscribe(path => _imageSources.SetDroppedPath(ContentIndex, path))
                .AddTo(CompositeDisposable);

            // VM←M
            // Listener系はIsActiveを参照して非表示時は無視するようにしています(◆より良い実装あれば変えたい)
            var sourceImageDirectoryCache =_imageSources.ImageDirectries
                .ObserveElementPropertyChanged()
                //.Do(x => Debug.WriteLine($"{x.EventArgs} {x.Sender}"))
                .Select(x => x.Sender)
                .Where(x => x.ContentIndex == ContentIndex)
                //.Do(x => Debug.WriteLine($"Log1: {ContentIndex}, {IsActive.Value}, {x.DirectoryPath}"))
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);

            var sourceImageDirectory = sourceImageDirectoryCache
                .CombineLatest(IsActive, (imageDirectory, isActive) => (imageDirectory, isActive))
                //.Do(x => Debug.WriteLine($"Log2: {ContentIndex}, {x.isActive}, {x.path}"))
                .Where(x => x.isActive).Select(x => x.imageDirectory)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);

            // Model画像ディレクトリ変化時の処理
            sourceImageDirectory
                .Subscribe(x => DirectoryPath.Value = x.DirectoryPath)
                .AddTo(CompositeDisposable);

            // 対象画像リストの読み出し(◆拡張性の判定が不十分)
            SourceImagesPath = sourceImageDirectory
                .Select(x => Directory.EnumerateFiles(x.DirectoryPath, "*.jpg", SearchOption.TopDirectoryOnly).ToList())
                .Cast<IReadOnlyList<string>>()
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);

            // 選択ディレクトリ更新(VM→M)
            SelectedImagePath
                .Subscribe(x => _imageSources.SetSelectedFlePath(ContentIndex, x))
                .AddTo(CompositeDisposable);

            // 選択ディレクトリ更新(VM←M)
            sourceImageDirectory
                .Subscribe(x => SelectedImagePath.Value = x.SelectedFilePath)
                .AddTo(CompositeDisposable);

        }

    }
}
