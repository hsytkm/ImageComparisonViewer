using ImageComparisonViewer.MainTabControl.ViewModels.Bases;
using Prism.Ioc;
using Prism.Regions;
using System.Diagnostics;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class TabContentDoubleViewModel : TabContentImageViewModelBase
    {
        private const string _title = "Double";
        private const int _index = 2;

        public TabContentDoubleViewModel(IContainerExtension container, IRegionManager regionManager)
            : base(container, regionManager, _title, _index)
        {
            Debug.WriteLine($"{nameof(TabContentDoubleViewModel): ctor}");
        }

    }
}
