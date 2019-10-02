using ImageComparisonViewer.ImagePanels.Views;
using ImageComparisonViewer.MainTabControl.Common;
using Prism.Ioc;
using Prism.Regions;
using System.Windows.Controls;

namespace ImageComparisonViewer.MainTabControl.Views
{
    /// <summary>
    /// TabContentSingle.xaml の相互作用ロジック
    /// </summary>
    public partial class TabContentSingle : UserControl
    {
        private static readonly int _contentCount = 1;

        public TabContentSingle(IContainerExtension container, IRegionManager regionManager)
        {
            InitializeComponent();

            // 以下で動くが他画像に合わせる
            //regionManager.RegisterViewWithRegion(RegionNames.ImageContentRegion1_0, typeof(ImagePanel));
            TabContentDouble.RegisterImagePanelViewsWithRegion(container, regionManager, _contentCount);
        }

    }
}
