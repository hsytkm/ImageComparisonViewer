using Control.ImagePanel.Views;
using ImageComparisonViewer.MainTabControl.Common;
using Prism.Ioc;
using Prism.Regions;
using System.Windows.Controls;

namespace ImageComparisonViewer.MainTabControl.Views
{
    /// <summary>
    /// TabContentTriple.xaml の相互作用ロジック
    /// </summary>
    public partial class TabContentTriple : UserControl
    {
        private static readonly int _contentCount = 3;

        public TabContentTriple(IContainerExtension container, IRegionManager regionManager)
        {
            InitializeComponent();

            TabContentDouble.RegisterImagePanelViewsWithRegion(container, regionManager, _contentCount);
        }
    }
}
