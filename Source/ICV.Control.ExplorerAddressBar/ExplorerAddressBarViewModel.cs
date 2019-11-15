using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core.Images;
using Prism.Commands;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace ICV.Control.ExplorerAddressBar
{
    class ExplorerAddressBarViewModel : DisposableBindableBase
    {
        // Modelディレクトリ選択が存在しない場合の初期ディレクトリ
        private static readonly string _defaultDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        #region Visibility
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
        #endregion

        /// <summary>対象ディレクトリ</summary>
        public ReactiveProperty<string> TargetDirectory { get; } =
            new ReactiveProperty<string>(mode: ReactivePropertyMode.DistinctUntilChanged);

        #region ItemsControl(DirectoryNodes)
        /// <summary>
        /// ディレクトリPATHをModelに通知するAction(子View用)
        /// </summary>
        private readonly Action<string> _sendSerectedDirectoryPathAction;

        /// <summary>
        /// View(ディレクトリノード)のリスト
        /// </summary>
        public ReactiveProperty<IList<DirectoryPathNode>> ViewItemsSource { get; } =
             new ReactiveProperty<IList<DirectoryPathNode>>(mode: ReactivePropertyMode.None);
        #endregion

        /// <summary>TextBox入力の確定コマンド</summary>
        public DelegateCommand<string> TextEnterCommand { get; } = default!;

        /// <summary>TextBox入力のキャンセルコマンド</summary>
        public DelegateCommand TextCancelCommand { get; } = default!;

        public ExplorerAddressBarViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            var compositeDirectory = container.Resolve<CompositeImageDirectory>();
            var imageDirectory = compositeDirectory.ImageDirectries[parameter.ContentIndex];

            #region Visibility

            VisibleTextBoxCommand = new DelegateCommand(() => IsVisibleDirectoryNode.TurnOff());
            CollapsedTextBoxCommand = new DelegateCommand(() => IsVisibleDirectoryNode.TurnOn());

            #endregion

            // directory from model
            imageDirectory
                .ObserveProperty(x => x.DirectoryPath)
                .Select(path => (path is null) ? _defaultDirectory : path)
                .Subscribe(path => DirectoryNode.EmendFullPathFromModel(path))
                .AddTo(CompositeDisposable);

            // directory to model
            TargetDirectory
                .Subscribe(path => imageDirectory.SetSelectedDictionaryPath(DirectoryNode.EmendFullPathFromViewModel(path)))
                .AddTo(CompositeDisposable);

            #region ItemsControl(DirectoryNodes)

            // ディレクトリPATHをModelに通知するAction(子View用)
            _sendSerectedDirectoryPathAction = path => TargetDirectory.Value = path;

            // Modelのディレクトリを購読
            var targetDirectory = imageDirectory
                .ObserveProperty(x => x.DirectoryPath)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.Default)
                .AddTo(CompositeDisposable);

            // ディレクトリViewの読込み
            targetDirectory
                .Subscribe(path => UpdateViewsSource(path is null ? _defaultDirectory : path))
                .AddTo(CompositeDisposable);

            #endregion

            // TextBox入力確定時のディレクトリ通知
            TextEnterCommand = new DelegateCommand<string>(path =>
            {
                if (Directory.Exists(path))
                    TargetDirectory.Value = path;

                IsVisibleDirectoryNode.TurnOn();
            });

            // キャンセル時は表示切替のみ(テキストボックスに入力中の文字は残している)
            TextCancelCommand = new DelegateCommand(() => IsVisibleDirectoryNode.TurnOn());

        }

        #region UpdateViewsSource
        /// <summary>
        /// Viewのディレクトリノードを更新
        /// </summary>
        /// <param name="sourcePath"></param>
        private void UpdateViewsSource(string? sourcePath = null)
        {
            if (ViewItemsSource.Value != null)
            {
                foreach (var item in ViewItemsSource.Value)
                {
                    if (item is IDisposable disposable)
                        disposable.Dispose();
                }
                ViewItemsSource.Value.Clear();
            }

            if (!string.IsNullOrEmpty(sourcePath))
            {
                ViewItemsSource.Value = DirectoryNode.GetDirectoryNodes(sourcePath)
                    .Select(node => new DirectoryPathNode(node, _sendSerectedDirectoryPathAction))
                    .ToList();
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            // ディレクトリなしで要求してコントロールを削除する!!
            UpdateViewsSource();

            base.Dispose(disposing);
        }

    }
}
