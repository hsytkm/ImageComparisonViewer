using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core.Images;
using Prism.Commands;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;

namespace ICV.Control.ExplorerAddressBar
{
    class ExplorerAddressBarViewModel : DisposableBindableBase
    {
        private static readonly string _defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        /// <summary>
        /// DirectoryNodeを表示する(TextBoxを表示しない)
        /// </summary>
        public ReactiveProperty<bool> IsVisibleDirectoryNode
        {
            get => _isVisibleDirectoryNode;
            private set => SetProperty(ref _isVisibleDirectoryNode, value);
        }
        private ReactiveProperty<bool> _isVisibleDirectoryNode = default!;

        /// <summary>
        /// 対象ディレクトリ
        /// </summary>
        public ReactiveProperty<string> TargetDirectory
        {
            get => _targetDirectory;
            private set => SetProperty(ref _targetDirectory, value);
        }
        private ReactiveProperty<string> _targetDirectory = default!;

        /// <summary>
        /// TextBoxの入力文字列(EnterKeyでViewから通知が来る)
        /// </summary>
        public ReactiveProperty<string> InputText
        {
            get => _inputText;
            private set => SetProperty(ref _inputText, value);
        }
        private ReactiveProperty<string> _inputText = default!;

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand<string> TextInputCommand
        {
            get => _textInputCommand;
            private set => SetProperty(ref _textInputCommand, value);
        }
        private DelegateCommand<string> _textInputCommand = default!;

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand VisibleTextBoxCommand
        {
            get => _visibleTextBoxCommand;
            private set => SetProperty(ref _visibleTextBoxCommand, value);
        }
        private DelegateCommand _visibleTextBoxCommand = default!;

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand CollapsedTextBoxCommand
        {
            get => _collapsedTextBoxCommand;
            private set => SetProperty(ref _collapsedTextBoxCommand, value);
        }
        private DelegateCommand _collapsedTextBoxCommand = default!;

        public ExplorerAddressBarViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            var compositeDirectory = container.Resolve<CompositeImageDirectory>();
            var imageDirectory = compositeDirectory.ImageDirectries[parameter.ContentIndex];

            IsVisibleDirectoryNode = new ReactiveProperty<bool>(initialValue: true)
                .AddTo(CompositeDisposable);

            InputText = new ReactiveProperty<string>().AddTo(CompositeDisposable);

            // Modelのディレクトリを購読
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
                    Debug.WriteLine($"ToModel:{path}");
                    TargetDirectory.Value = path;
                }
                IsVisibleDirectoryNode.Value = true;
            });

            VisibleTextBoxCommand = new DelegateCommand(() => IsVisibleDirectoryNode.Value = false);
            CollapsedTextBoxCommand = new DelegateCommand(() => IsVisibleDirectoryNode.Value = true);

            // TextBox表示時に文字列を準備
            IsVisibleDirectoryNode
                .Where(x => !x)
                .Subscribe(_ => InputText.Value = TargetDirectory.Value ?? "")
                .AddTo(CompositeDisposable);
        }

    }
}
