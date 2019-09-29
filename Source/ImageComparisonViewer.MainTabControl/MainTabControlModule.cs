using ImageComparisonViewer.MainTabControl.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;

namespace ImageComparisonViewer.MainTabControl
{
    public class MainTabControlModule : IModule
    {
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
            var regionManager = containerProvider.Resolve<IRegionManager>();
            var region = regionManager.Regions["MainTabContentRegion"];

            foreach (var type in _tabContentViewsType)
            {
                var view = containerProvider.Resolve(type);
                region.Add(view);
            }
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterSingleton<ahe>();
        }
    }
}
