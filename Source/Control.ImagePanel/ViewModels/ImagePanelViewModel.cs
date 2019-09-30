using ImageComparisonViewer.Core;
using Prism.Ioc;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Control.ImagePanel.ViewModels
{
    class ImagePanelViewModel : BindableBase
    {
        private readonly ImageSources _imageSources = default!;

        private int _contentIndex;

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

        public ImagePanelViewModel(IContainerExtension container)
        {
            _imageSources = container.Resolve<ImageSources>();

            DropEvent
                .Subscribe(paths =>
                {
                    //foreach (var path in paths) Debug.WriteLine(path);

                    // 先頭PATHを採用
                    if (paths.Any()) DirectoryPath.Value = paths.First();
                });
                //.AddTo(CompositeDisposable);

            DirectoryPath
                .Subscribe(path => _imageSources.SetDirectryPath(_contentIndex, path));
                //.AddTo(CompositeDisposable);
        }

        public void SetContentIndex(int contentIndex)
        {
            _contentIndex = contentIndex;
            UpdateImageSource(contentIndex);
        }

        private void UpdateImageSource(int contentIndex)
        {
            DirectoryPath.Value = _imageSources.GetDirectryPath(contentIndex);
        }

    }
}
