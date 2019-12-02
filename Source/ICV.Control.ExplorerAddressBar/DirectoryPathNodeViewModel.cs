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
        /// ディレクトリ選択コマンド
        /// </summary>
        public DelegateCommand<DirectoryNode> DirectorySelectCommand { get; } = default!;

        /// <summary>
        /// ディレクトリ選択(OneWayToSource)
        /// </summary>
        public ReactiveProperty<DirectoryNode> SelectedNode { get; } =
            new ReactiveProperty<DirectoryNode>(mode: ReactivePropertyMode.DistinctUntilChanged);

        public DirectoryPathNodeViewModel(DirectoryNode targetNode, Action<string> sendDirectoryPath)
        {
            if (sendDirectoryPath is null) throw new ArgumentNullException(nameof(sendDirectoryPath));

            TargetNode = targetNode;

            ChildDirectories = targetNode.GetChildDirectoryNodes();

            DirectorySelectCommand = new DelegateCommand<DirectoryNode>(x => SelectedNode.Value = x);

            // Viewのディレクトリ選択を外部に通知
            SelectedNode
                .Subscribe(node => sendDirectoryPath(node.FullPath))
                .AddTo(CompositeDisposable);
        }

    }
}
