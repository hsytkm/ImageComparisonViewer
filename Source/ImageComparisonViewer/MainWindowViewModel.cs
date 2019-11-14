using ImageComparisonViewer.Common.Prism;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Diagnostics;

namespace ImageComparisonViewer
{
    class MainWindowViewModel : BindableBase
    {
        public IApplicationCommands ApplicationCommands { get; }

        public MainWindowViewModel(/*IRegionManager regionManager,*/ IApplicationCommands applicationCommands)
        {
            ApplicationCommands = applicationCommands;
        }
    }
}
