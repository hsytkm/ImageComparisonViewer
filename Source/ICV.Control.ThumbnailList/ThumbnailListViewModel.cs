﻿using ICV.Control.ThumbnailList.EventConverters;
using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Linq;
using System.Reactive.Linq;

namespace ICV.Control.ThumbnailList
{
    class ThumbnailListViewModel : DisposableBindableBase
    {
        /// <summary>
        /// サムネイルリスト
        /// </summary>
        public ReadOnlyReactiveCollection<Thumbnail> ThumbnailSources
        {
            get => _thumbnailSources;
            private set => SetProperty(ref _thumbnailSources, value);
        }
        private ReadOnlyReactiveCollection<Thumbnail> _thumbnailSources = default!;

        /// <summary>
        /// 選択中のサムネイル(未選択時はnull)
        /// </summary>
        public ReactiveProperty<Thumbnail?> SelectedItem
        {
            get => _selectedItem;
            private set => SetProperty(ref _selectedItem, value);
        }
        private ReactiveProperty<Thumbnail?> _selectedItem = default!;

        /// <summary>
        /// ScrollViewerの表示範囲割合
        /// </summary>
        public ReactiveProperty<HorizontalScrolltRatio> ScrollViewerHorizontalScrollRatio
        {
            get => _scrollViewerHorizontalScrollRatio;
            private set => SetProperty(ref _scrollViewerHorizontalScrollRatio, value);
        }
        private ReactiveProperty<HorizontalScrolltRatio> _scrollViewerHorizontalScrollRatio = default!;

        public ThumbnailListViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            var imageSources = container.Resolve<ImageSources>();
            var imageSource = imageSources.ImageDirectries[parameter.ContentIndex];

            // 選択候補サムネイル要素の作成
            ThumbnailSources = imageSource.ImageFiles
                .ToReadOnlyReactiveCollection(x => new Thumbnail(x.FilePath))
                .AddTo(CompositeDisposable);

#pragma warning disable CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。
            // 選択中画像のTwoWay
            SelectedItem = imageSource
                .ToReactivePropertyAsSynchronized(x => x.SelectedFilePath,
                    convert: m => ThumbnailSources.FirstOrDefault(x => x.FilePath == m),
                    convertBack: v => v?.FilePath,
                    mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);
#pragma warning restore CS8619 // 値における参照型の Null 許容性が、対象の型と一致しません。

            // サムネイルScrollの水平表示領域
            ScrollViewerHorizontalScrollRatio = new ReactiveProperty<HorizontalScrolltRatio>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            ScrollViewerHorizontalScrollRatio
                .Subscribe(x => imageSource.UpdateThumbnail(x.CenterRatio, x.ViewportRatio))
                .AddTo(CompositeDisposable);

            // Modelのサムネイル読み込みを監視して更新
            imageSource.ImageFiles
                .ObserveElementProperty(x => x.Thumbnail)
                //.Do(x => Debug.WriteLine($"{x.Instance} {x.Property} {x.Value}"))
                .Subscribe(model =>
                {
                    var vmodel = ThumbnailSources.FirstOrDefault(x => x.FilePath == model.Instance.FilePath);
                    if (vmodel != null)
                        vmodel.Image = model.Instance.Thumbnail;
                })
                .AddTo(CompositeDisposable);

        }

    }
}
