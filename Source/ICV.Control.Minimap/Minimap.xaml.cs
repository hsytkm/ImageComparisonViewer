using ImageComparisonViewer.Common.Mvvm;
using Prism.Ioc;

namespace ICV.Control.Minimap
{
    /// <summary>
    /// Interaction logic for Minimap.xaml
    /// </summary>
    public partial class Minimap : DisposableUserControl
    {
        public Minimap(IContainerExtension container, ImageViewParameter parameter)
        {
            // VMに引数を渡したいので自前でインスタンス作る
            DataContext = new MinimapViewModel(container, parameter);

            InitializeComponent();
        }
    }
}
