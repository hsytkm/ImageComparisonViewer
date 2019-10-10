using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core.Images;
using Prism.Commands;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Diagnostics;
using System.IO;

namespace ICV.Control.ExplorerAddressBar
{
    class ExplorerAddressBarViewModel : DisposableBindableBase
    {
        private static readonly string _defaultDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        /// <summary>
        /// DirectoryNodeを表示する(TextBoxを表示しない)
        /// </summary>
        public BooleanNotifier IsVisibleDirectoryNode { get; } = new BooleanNotifier(initialValue: true);

        /// <summary>
        /// TextBoxを表示する(DirectoryNodeを非表示にする)
        /// </summary>
        public DelegateCommand VisibleTextBoxCommand { get; } = default!;

        /// <summary>
        /// TextBoxを非表示にする(DirectoryNodeを表示する)
        /// </summary>
        public DelegateCommand CollapsedTextBoxCommand { get; } = default!;

        /// <summary>
        /// 対象ディレクトリ
        /// </summary>
        public ReactiveProperty<string> TargetDirectory { get; } = default!;

        /// <summary>
        /// TextBox確定時のコマンド
        /// </summary>
        public DelegateCommand<string> TextInputCommand { get; } = default!;

        public ExplorerAddressBarViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            var compositeDirectory = container.Resolve<CompositeImageDirectory>();
            var imageDirectory = compositeDirectory.ImageDirectries[parameter.ContentIndex];

            VisibleTextBoxCommand = new DelegateCommand(() => IsVisibleDirectoryNode.TurnOff());
            CollapsedTextBoxCommand = new DelegateCommand(() => IsVisibleDirectoryNode.TurnOn());

            // Modelのディレクトリ(TwoWay)
            TargetDirectory = imageDirectory
                .ToReactivePropertyAsSynchronized(x => x.DirectoryPath,
                    convert: m => (m is null) ? _defaultDirectory : m,
                    convertBack: v => v,
                    mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            // TextBox入力確定時の処理
            TextInputCommand = new DelegateCommand<string>(path =>
            {
                if (Directory.Exists(path))
                {
                    TargetDirectory.Value = path;
                }
                IsVisibleDirectoryNode.TurnOn();
            });
        }

    }
}
