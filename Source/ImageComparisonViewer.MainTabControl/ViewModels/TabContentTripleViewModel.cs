using ImageComparisonViewer.MainTabControl.ViewModels.Bases;
using Prism.Ioc;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class TabContentTripleViewModel : TabContentImageViewModelBase
    {
        private readonly static string _title = "Triple";
        private readonly static int _index = 3;

        public TabContentTripleViewModel(IContainerExtension container, IRegionManager regionManager)
            : base(container, regionManager, _title, _index)
        {
            Debug.WriteLine($"{nameof(TabContentTripleViewModel): ctor}");
        }

    }
}
