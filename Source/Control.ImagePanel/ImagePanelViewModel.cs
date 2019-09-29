using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Control.ImagePanel
{
    class ImagePanelViewModel : BindableBase
    {

        public string DirectoryPath
        {
            get => _directoryPath;
            set
            {
                if (SetProperty(ref _directoryPath, value))
                    Debug.WriteLine($"ImagePanel Directory: {value}");
            }
        }
        private string _directoryPath;


    }
}
