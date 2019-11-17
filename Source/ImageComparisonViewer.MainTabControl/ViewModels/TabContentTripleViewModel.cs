using ImageComparisonViewer.Common.Prism;
using ImageComparisonViewer.MainTabControl.ViewModels.Bases;
using Prism.Ioc;
using Prism.Regions;
using System.Diagnostics;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class TabContentTripleViewModel : TabContentImageViewModelBase
    {
        private const string _title = "Triple";
        private const int _index = 3;

        public TabContentTripleViewModel(IContainerExtension container, IRegionManager regionManager)
            : base(container, regionManager, _title, _index)
        {
            Debug.WriteLine($"{nameof(TabContentTripleViewModel): ctor}");
        }

    }
}
