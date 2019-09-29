using ImageComparisonViewer.MainTabControl.ViewModels.Bases;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class TabContentSingleViewModel : TabContentImageViewModelBase
    {
        private readonly static string _title = "Single";
        private readonly static int _index = 1;

        public TabContentSingleViewModel(IRegionManager regionManager)
            : base(regionManager, _title, _index)
        {
            Debug.WriteLine($"{nameof(TabContentSingleViewModel): ctor}");
        }

    }
}
