using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;

namespace ImageComparisonViewer.Core
{
    public class CoreModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // MainTabControlでSingletonインスタンスを取得できなかったので、
            // ImageComparisonViewerで登録するように変更した
            //containerRegistry.RegisterSingleton<ImageSources>();
        }
    }
}
