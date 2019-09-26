using Prism.Mvvm;
using System;
using System.Diagnostics;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class MainTabContentSingleViewModel : BindableBase
    {
        public string Title => "Single";
        public string Message => "Message Single";

        public MainTabContentSingleViewModel()
        {
            Debug.WriteLine($"{nameof(MainTabContentSingleViewModel): ctor}");
        }

    }
}
