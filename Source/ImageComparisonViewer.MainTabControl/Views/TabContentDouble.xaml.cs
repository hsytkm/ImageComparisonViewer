using Control.ImagePanel.Views;
using ImageComparisonViewer.MainTabControl.Common;
using Prism.Ioc;
using Prism.Regions;
using System.Windows.Controls;

namespace ImageComparisonViewer.MainTabControl.Views
{
    /// <summary>
    /// TabContentDouble.xaml の相互作用ロジック
    /// </summary>
    public partial class TabContentDouble : UserControl
    {
        private static readonly int _contentCount = 2;

        public TabContentDouble(IContainerExtension container, IRegionManager regionManager)
        {
            InitializeComponent();

            foreach (var name in RegionNames.GetImageContentRegionNames(_contentCount))
            {
                regionManager.RegisterViewWithRegion(name,
                    () => container.Resolve<ImagePanel>());
            }
        }
    }
}
