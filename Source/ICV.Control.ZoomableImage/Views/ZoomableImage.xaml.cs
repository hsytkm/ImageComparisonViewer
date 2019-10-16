using ICV.Control.ZoomableImage.ViewModels;
using ImageComparisonViewer.Common.Mvvm;
using Prism.Ioc;
using System.Windows.Controls;

namespace ICV.Control.ZoomableImage.Views
{
    /// <summary>
    /// ZoomableImage.xaml の相互作用ロジック
    /// </summary>
    public partial class ZoomableImage : DisposableUserControl
    {
        public ZoomableImage(IContainerExtension container, ImageViewParameter parameter)
        {
            // VMに引数を渡したいので自前でインスタンス作る
            DataContext = new ZoomableImageViewModel(container, parameter);

            InitializeComponent();
        }
    }
}
