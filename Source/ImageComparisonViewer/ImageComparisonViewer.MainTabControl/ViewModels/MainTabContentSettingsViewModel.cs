using Prism.Mvvm;
using System;
using System.Diagnostics;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class MainTabContentSettingsViewModel : BindableBase
    {
        public string Title => "Settings";
        public string Message => "Message Settings";

        public MainTabContentSettingsViewModel()
        {
            Debug.WriteLine($"{nameof(MainTabContentSettingsViewModel): ctor}");
        }

    }
}
