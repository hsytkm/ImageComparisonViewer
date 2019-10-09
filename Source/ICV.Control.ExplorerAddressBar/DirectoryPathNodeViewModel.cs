using ImageComparisonViewer.Common.Mvvm;
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

        public DirectoryPathNodeViewModel(DirectoryNode targetNode)
        {
            TargetNode = targetNode;
            ChildDirectories = targetNode.GetChildDirectoryNodes();

            DirectorySelectCommand = new DelegateCommand<DirectoryNode>(node =>
                {
                    Debug.WriteLine($"Command: {node.FullPath}");
                });

            SelectedNode = new ReactiveProperty<DirectoryNode>(mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);
            SelectedNode
                .Subscribe(node => Debug.WriteLine($"SelectedNode: {node.FullPath}"))
                .AddTo(CompositeDisposable);

        }

    }
}
