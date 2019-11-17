using ICV.Control.ThumbnailList.EventConverters;
using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core.Images;
using Prism.Commands;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace ICV.Control.ThumbnailList
{
    class ThumbnailListViewModel : DisposableBindableBase
    {
        /// <summary>
        /// サムネイルリスト
        /// </summary>
        public ReadOnlyReactiveCollection<Thumbnail> ThumbnailSources { get; } = default!;

        /// <summary>
        /// 選択中のサムネイル(未選択時はnull)
        /// </summary>
        public ReactiveProperty<Thumbnail?> SelectedItem { get; } = default!;

        /// <summary>
        /// ScrollViewerの表示範囲割合
        /// </summary>
        public ReactiveProperty<HorizontalScrolltRatio> ScrollViewerHorizontalScrollRatio { get; } =
            new ReactiveProperty<HorizontalScrolltRatio>(mode: ReactivePropertyMode.DistinctUntilChanged);

        /// <summary>
        /// 選択画像を進めるコマンド
        /// </summary>
        public DelegateCommand NextSelectedItemCommand { get; } = default!;

        /// <summary>
        /// 選択画像を戻すコマンド
        /// </summary>
        public DelegateCommand PrevSelectedItemCommand { get; } = default!;

        public ThumbnailListViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            var compositeDirectory = container.Resolve<ICompositeImageDirectory>();
            var imageDirectory = compositeDirectory.ImageDirectries[parameter.ContentIndex];

            // 選択候補サムネイル要素の作成
            ThumbnailSources = imageDirectory.ImageFiles
                .ToReadOnlyReactiveCollection(x => new Thumbnail(x.FilePath), scheduler: Scheduler.CurrentThread)
                .AddTo(CompositeDisposable);

#pragma warning disable CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。
            // 選択中画像のTwoWay
            SelectedItem = imageDirectory
                .ToReactivePropertyAsSynchronized(x => x.SelectedFilePath,
                    convert: m => ThumbnailSources.FirstOrDefault(x => x.FilePath == m),
                    convertBack: vm => vm?.FilePath,
                    mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);
#pragma warning restore CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。

            // サムネイルScrollの水平表示領域
            ScrollViewerHorizontalScrollRatio
                .Subscribe(x => imageDirectory.UpdateThumbnails(x.CenterRatio, x.ViewportRatio))
                .AddTo(CompositeDisposable);

            // Modelのサムネイル読み込みを監視して更新
            imageDirectory.ImageFiles
                .ObserveElementProperty(x => x.Thumbnail)
                //.Do(x => Debug.WriteLine($"{x.Instance} {x.Property} {x.Value}"))
                .Subscribe(model =>
                {
                    var vmodel = ThumbnailSources.FirstOrDefault(x => x.FilePath == model.Instance.FilePath);
                    if (vmodel != null)
                        vmodel.Image = model.Instance.Thumbnail;
                })
                .AddTo(CompositeDisposable);

            // 選択画像の進む/戻る
            NextSelectedItemCommand = new DelegateCommand(imageDirectory.MoveNextImage);
            PrevSelectedItemCommand = new DelegateCommand(imageDirectory.MovePrevImage);

        }

    }
}
