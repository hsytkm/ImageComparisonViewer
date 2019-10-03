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
        /// ディレクトリPATH
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
        public ReadOnlyReactiveProperty<IReadOnlyCollection<string>> SourceImagesPath { get; }

        /// <summary>
        /// 選択中の画像PATH
        /// </summary>
        public ReactiveProperty<string> SelectedImagePath { get; } = new ReactiveProperty<string>();

        public ImagePanelViewModel(IContainerExtension container)
        {
            _imageSources = container.Resolve<ImageSources>();

            // VM->M
            DropEvent
                .Subscribe(paths => _imageSources.SetDirectriesPath(ContentIndex, paths))
                .AddTo(CompositeDisposable);

            // VM->M
            DirectoryPath
                .Subscribe(path => _imageSources.SetDirectryPath(ContentIndex, path))
                .AddTo(CompositeDisposable);

            // VM<-M
            // Listener系はIsActiveを参照して非表示時は無視するようにしています(◆より良い実装あれば変えたい)
            var sourceDirectory = _imageSources.DirectriesPath
                .CollectionChangedAsObservable()
                .Where(e => e.Action == NotifyCollectionChangedAction.Replace)
                .Where(e => e.NewStartingIndex == ContentIndex)
                //.Do(e => Debug.WriteLine($"Log1: {ContentIndex}, {IsActive.Value}, {e.NewStartingIndex}, {e.NewItems.Cast<string>().First()}"))
                .Select(e => e.NewItems.Cast<string>().First())
                .ToReadOnlyReactiveProperty()
                .AddTo(CompositeDisposable);
            sourceDirectory
                .CombineLatest(IsActive, (path, isActive) => (path, isActive))
                //.Do(x => Debug.WriteLine($"Log2: {ContentIndex}, {x.isActive}, {x.path}"))
                .Where(x => x.isActive).Select(x => x.path)
                .Subscribe(x => DirectoryPath.Value = x)
                .AddTo(CompositeDisposable);

            // ToDo:対象画像リストの読み出し
            SourceImagesPath = DirectoryPath
                .Select(x => Directory.EnumerateFiles(x, "*", SearchOption.TopDirectoryOnly).ToList())
                .Do(x => Debug.WriteLine(x))
                .Cast<IReadOnlyCollection<string>>()
                .ToReadOnlyReactiveProperty();

        }

    }
}
