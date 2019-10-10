using ImageComparisonViewer.Common.Mvvm;
using Prism.Ioc;

namespace ICV.Control.ThumbnailList
{
    /// <summary>
    /// ThumbnailList.xaml の相互作用ロジック
    /// </summary>
    public partial class ThumbnailList : DisposableUserControl
    {
        public ThumbnailList(IContainerExtension container, ImageViewParameter parameter)
        {
            // VMに引数を渡したいので自前でインスタンス作る
            DataContext = new ThumbnailListViewModel(container, parameter);

            InitializeComponent();
        }
    }
}
