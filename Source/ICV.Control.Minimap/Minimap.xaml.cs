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
            // VM�Ɉ�����n�������̂Ŏ��O�ŃC���X�^���X���
            DataContext = new MinimapViewModel(container, parameter);

            InitializeComponent();
        }
    }
}
