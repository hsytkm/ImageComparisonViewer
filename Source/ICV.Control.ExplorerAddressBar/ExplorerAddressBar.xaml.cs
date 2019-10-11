using ImageComparisonViewer.Common.Mvvm;
using Prism.Ioc;

namespace ICV.Control.ExplorerAddressBar
{
    /// <summary>
    /// Interaction logic for ExplorerAddressBar.xaml
    /// </summary>
    public partial class ExplorerAddressBar : DisposableUserControl
    {
        public ExplorerAddressBar(IContainerExtension container, ImageViewParameter parameter)
        {
            // VMに引数を渡したいので自前でインスタンス作る
            DataContext = new ExplorerAddressBarViewModel(container, parameter);

            InitializeComponent();
        }
    }
}
