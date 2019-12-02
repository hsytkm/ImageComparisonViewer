using Prism.Mvvm;
using System;

namespace ImageComparisonViewer.Core.Settings
{
    public class UserSettings : BindableBase
    {
        /// <summary>ユーザ操作の連動フラグ</summary>
        public bool IsControlInterlock
        {
            get => _isControlInterlock;
            set => SetProperty(ref _isControlInterlock, value);
        }
        private bool _isControlInterlock = true;

        public UserSettings() { }

    }
}
