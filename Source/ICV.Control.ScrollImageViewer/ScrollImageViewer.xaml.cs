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
            // VM�Ɉ�����n�������̂Ŏ��O�ŃC���X�^���X���
            DataContext = new ScrollImageViewerViewModel(container, parameter);

            InitializeComponent();
        }
    }
}