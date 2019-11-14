using ImageComparisonViewer.Common.Prism;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Diagnostics;

namespace ImageComparisonViewer
{
    class MainWindowViewModel : BindableBase
    {
        public IApplicationCommands ApplicationCommands { get; }

        public MainWindowViewModel(IRegionManager regionManager, IApplicationCommands applicationCommands)
        {
            ApplicationCommands = applicationCommands;

            //NavigatedToTabContent(regionManager);
        }

        //private async Task NavigatedToTabContent(IRegionManager regionManager)
        //{
        //    await Task.Delay(10 * 1000);
        //    regionManager.RequestNavigate("MainTabContentRegion", "TabContentSettings");
        //}

    }
}
