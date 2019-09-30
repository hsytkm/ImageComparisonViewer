﻿using Control.ImagePanel.Views;
using ImageComparisonViewer.MainTabControl.Common;
using Prism.Ioc;
using Prism.Regions;
using System.Linq;
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

            RegisterImagePanelViewsWithRegion(container, regionManager, _contentCount);
        }

        /// <summary>
        /// RegionにViewを登録する
        /// </summary>
        /// <param name="container"></param>
        /// <param name="regionManager"></param>
        /// <param name="contentCount">画像コンテンツの最大数</param>
        internal static void RegisterImagePanelViewsWithRegion(
            IContainerExtension container, IRegionManager regionManager, int contentCount)
        {
            var regionNames = RegionNames.GetImageContentRegionNames(contentCount);
            foreach (var (name, index) in regionNames.Select((name, index) => (name, index)))
            {
                // ◆複数の引数を渡す場合はデータstructに変えましょう
                var parameters = new[] { (typeof(int), (object)index) };
                regionManager.RegisterViewWithRegion(name,
                    () => container.Resolve<ImagePanel>(parameters));
            }
        }

    }
}
