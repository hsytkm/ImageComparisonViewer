using ImageComparisonViewer.Common.Prism;
using ImageComparisonViewer.MainTabControl.ViewModels.Bases;
using Prism.Ioc;
using Prism.Regions;
using System.Diagnostics;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class TabContentSingleViewModel : TabContentImageViewModelBase
    {
        private const string _title = "Single";
        private const int _index = 1;

        public TabContentSingleViewModel(IContainerExtension container, IRegionManager regionManager)
            : base(container, regionManager, _title, _index)
        {
            Debug.WriteLine($"{nameof(TabContentSingleViewModel): ctor}");
        }

    }
}
