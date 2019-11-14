using ImageComparisonViewer.Common.Prism;
using ImageComparisonViewer.MainTabControl.Common;
using ImageComparisonViewer.MainTabControl.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;

namespace ImageComparisonViewer.MainTabControl
{
    public class MainTabControlModule : IModule
    {
        private static readonly string MainTabContentRegion = nameof(MainTabContentRegion);
        private static readonly int _imageCountMax = 3;  // Tripleまで対応

        // Tabに登録するViews
        private static readonly Type[] _tabContentViewsType = new[]
        {
            typeof(TabContentSingle),
            typeof(TabContentDouble),
            typeof(TabContentTriple),
            typeof(TabContentSettings),
        };

        public void OnInitialized(IContainerProvider containerProvider)
        {
            // TabContentの登録
            var regionManager = containerProvider.Resolve<IRegionManager>();
            var region = regionManager.Regions[MainTabContentRegion];
            foreach (var type in _tabContentViewsType)
            {
                region.Add(containerProvider.Resolve(type));
            }

            // Tab切り替えコマンドの登録
            var applicationCommands = containerProvider.Resolve<IApplicationCommands>();
            applicationCommands.NavigateImageTabContent.RegisterCommand(
                new DelegateCommand<int?>(index => NativateImageTabContent(regionManager, index)));

            // 初期化時のタブの画像表示数に切り替え
            NativateImageTabContent(regionManager, applicationCommands.OnInitializedTabContentImageCount);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry) { }

        /// <summary>
        /// 画像タブを移行させる
        /// </summary>
        /// <param name="regionManager"></param>
        /// <param name="tabIndex">切り替え後のTab指定(1画面=1, null=切替えなし)</param>
        private static void NativateImageTabContent(IRegionManager regionManager, int? tabIndex)
        {
            if (!tabIndex.HasValue) return;
            var index = tabIndex.Value;

            if (index < 0 || _imageCountMax < index)
                throw new IndexOutOfRangeException(index.ToString());

            var tabName = RegionNames.GetTabContentName(index);
            regionManager.RequestNavigate(MainTabContentRegion, tabName);
        }

    }
}
