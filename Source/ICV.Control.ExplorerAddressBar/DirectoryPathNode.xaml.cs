using ImageComparisonViewer.Common.Mvvm;
using System;

namespace ICV.Control.ExplorerAddressBar
{
    /// <summary>
    /// DirectoryPathNode.xaml の相互作用ロジック
    /// </summary>
    public partial class DirectoryPathNode : DisposableUserControl
    {
        public DirectoryPathNode(DirectoryNode node, Action<string> sendDirectoryPath)
        {
            // InitializeComponent()の前にViewModel作ること(for OneTime Binding)
            DataContext = new DirectoryPathNodeViewModel(node, sendDirectoryPath);

            InitializeComponent();
        }
    }
}
