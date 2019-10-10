using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core.Images;

namespace ICV.Control.ExplorerAddressBar
{
    /// <summary>
    /// DirectoryPathNode.xaml の相互作用ロジック
    /// </summary>
    public partial class DirectoryPathNode : DisposableUserControl
    {
        public DirectoryPathNode(DirectoryNode node, ImageDirectory imageDirectory)
        {
            // InitializeComponent()の前にViewModel作ること(for OneTime Binding)
            DataContext = new DirectoryPathNodeViewModel(node, imageDirectory);

            InitializeComponent();
        }
    }
}
