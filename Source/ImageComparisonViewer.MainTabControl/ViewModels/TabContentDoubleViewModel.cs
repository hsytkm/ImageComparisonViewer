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
    class TabContentDoubleViewModel : TabContentImageViewModelBase
    {
        private readonly static string _title = "Double";
        private readonly static int _index = 2;

        public TabContentDoubleViewModel(IContainerExtension container, IRegionManager regionManager)
            : base(container, regionManager, _title, _index)
        {
            Debug.WriteLine($"{nameof(TabContentDoubleViewModel): ctor}");
        }

    }
}
