using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Common.Prism;
using ImageComparisonViewer.Core.Images;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;

namespace ImageComparisonViewer.ImagePanels.ViewModels
{
    class ImagePanelViewModel : DisposableBindableBase
    {
        public int ContentIndex { get; }
        public int ContentCount { get; }

        /// <summary>
        /// 画像Drop時のイベント通知
        /// </summary>
        public ReactiveProperty<IReadOnlyList<string>> DropEvent { get; } =
            new ReactiveProperty<IReadOnlyList<string>>(mode: ReactivePropertyMode.None);

        /// <summary>
        /// ディレクトリPATH(未選択ならnull)
        /// </summary>
        public ReadOnlyReactiveProperty<string?> DirectoryPath { get; } = default!;

        /// <summary>
        /// 選択中の画像PATH(未選択ならnull)
        /// </summary>
        public ReadOnlyReactiveProperty<string?> SelectedImagePath { get; } = default!;

        public ImagePanelViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            ContentIndex = parameter.ContentIndex;
            ContentCount = parameter.ContentCount;

            var applicationCommands = container.Resolve<IApplicationCommands>();
            var compositeDirectory = container.Resolve<ICompositeImageDirectory>();
            var imageDirectory = compositeDirectory.ImageDirectries[ContentIndex];

            // ドロップファイル通知(ドロップ数に応じたTabに移行する)
            DropEvent
                .Subscribe(paths =>
                {
                    var tabImageUpdate = compositeDirectory.SetDroppedPaths(ContentIndex, paths);
                    applicationCommands.NavigateImageTabContent.Execute(tabImageUpdate);
                })
                .AddTo(CompositeDisposable);

            // 読み出しディレクトリ購読(デバッグ用)
            DirectoryPath = imageDirectory
                .ObserveProperty(x => x.DirectoryPath)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.None)
                .AddTo(CompositeDisposable);

            // 選択ファイル購読(デバッグ用)
            SelectedImagePath = imageDirectory
                .ObserveProperty(x => x.SelectedFilePath)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

        }

    }
}
