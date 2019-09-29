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
        /// <summary>
        /// ディレクトリPATH
        /// </summary>
        public string DirectoryPath
        {
            get => _directoryPath;
            set => SetProperty(ref _directoryPath, value);
        }
        private string _directoryPath = "";

        /// <summary>
        /// 画像Drop時のイベント通知
        /// </summary>
        public ReactiveProperty<IReadOnlyList<string>> DropEvent { get; } =
            new ReactiveProperty<IReadOnlyList<string>>(mode: ReactivePropertyMode.None);

        public ImagePanelViewModel()
        {
            DropEvent
                .Subscribe(paths =>
                {
                    //foreach (var path in paths) Debug.WriteLine(path);

                    // 先頭PATHを採用
                    if (paths.Any()) DirectoryPath = paths.First();
                });
                //.AddTo(CompositeDisposable);

        }

    }
}
