using Prism.Mvvm;
using Prism.Regions;
using System;

namespace ImageComparisonViewer
{
    class MainWindowViewModel : BindableBase
    {
        public MainWindowViewModel(IRegionManager regionManager)
        {
            //NavigatedToTabContent(regionManager);
        }

        //private async Task NavigatedToTabContent(IRegionManager regionManager)
        //{
        //    await Task.Delay(10 * 1000);
        //    regionManager.RequestNavigate("MainTabContentRegion", "TabContentSettings");
        //}

    }
}
