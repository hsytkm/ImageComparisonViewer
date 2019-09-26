using ImageComparisonViewer.MainTabControl.ViewModels.Bases;
using Prism.Mvvm;
using System;
using System.Diagnostics;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class TabContentSettingsViewModel : TabContentViewModelBase
    {
        private readonly static string _title = "Settings";

        public TabContentSettingsViewModel() : base(_title)
        {
            Debug.WriteLine($"{nameof(TabContentSettingsViewModel): ctor}");
        }

    }
}
