using ICV.Control.ScrollImageViewer.ViewModels;
using ImageComparisonViewer.Common.Mvvm;
using Prism.Ioc;

namespace ICV.Control.ScrollImageViewer
{
    /// <summary>
    /// Interaction logic for ScrollImageViewer.xaml
    /// </summary>
    public partial class ScrollImageViewer : DisposableUserControl
    {
        public ScrollImageViewer(IContainerExtension container, ImageViewParameter parameter)
        {
            // VMに引数を渡したいので自前でインスタンス作る
            DataContext = new ScrollImageViewerViewModel(container, parameter);

            InitializeComponent();
        }
    }
}