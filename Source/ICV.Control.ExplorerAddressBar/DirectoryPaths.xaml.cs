using ImageComparisonViewer.Common.Mvvm;
using Prism.Ioc;

namespace ICV.Control.ExplorerAddressBar
{
    /// <summary>
    /// DirectoryPaths.xaml の相互作用ロジック
    /// </summary>
    public partial class DirectoryPaths : DisposableUserControl
    {
        public DirectoryPaths(IContainerExtension container, ImageViewParameter parameter)
        {
            InitializeComponent();

            DataContext = new DirectoryPathsViewModel(container, parameter);
        }
    }
}
