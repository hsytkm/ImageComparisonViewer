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

            // 選択中画像 from Model
            SelectedItem = imageDirectory
                .ObserveProperty(x => x.SelectedFilePath)
                .Select(x => (x is null) ? default : ThumbnailSources.FirstOrDefault(y => y.FilePath == x))
                .ToReactiveProperty(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            // 選択中画像 to Model
            SelectedItem
                .Where(x => x != null)  // 本コントロールViewは画像未選択にできない
                .Cast<Thumbnail>()      // nullなし型に変換
                .Subscribe(x => imageDirectory.SetDroppedFilePath(x.FilePath))
                .AddTo(CompositeDisposable);

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
