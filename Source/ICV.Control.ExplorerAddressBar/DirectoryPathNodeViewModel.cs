using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core.Images;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

namespace ICV.Control.ExplorerAddressBar
{
    class DirectoryPathNodeViewModel : DisposableBindableBase
    {
        /// <summary>
        /// Viewの元ディレクトリ情報
        /// </summary>
        public DirectoryNode TargetNode { get; } = default!;

        /// <summary>
        /// 子ディレクトリのコレクション
        /// </summary>
        public IEnumerable<DirectoryNode> ChildDirectories { get; } = default!;

        /// <summary>
        /// ディレクトリ選択コマンド(Button)
        /// </summary>
        public DelegateCommand<DirectoryNode> DirectorySelectCommand { get; } = default!;

        /// <summary>
        /// ディレクトリ選択(ComboBox)
        /// </summary>
        public ReactiveProperty<DirectoryNode> SelectedNode
        {
            get => _selectedNode;
            private set => SetProperty(ref _selectedNode, value);
        }
        private ReactiveProperty<DirectoryNode> _selectedNode = default!;

        public DirectoryPathNodeViewModel(DirectoryNode targetNode, ImageDirectory imageDirectory)
        {
            TargetNode = targetNode;
            ChildDirectories = targetNode.GetChildDirectoryNodes();

            SelectedNode = new ReactiveProperty<DirectoryNode>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);
            SelectedNode
                .Subscribe(node => imageDirectory.DirectoryPath = node.FullPath)
                .AddTo(CompositeDisposable);

            DirectorySelectCommand = new DelegateCommand<DirectoryNode>(node =>
                SelectedNode.Value = node);
        }

    }
}
