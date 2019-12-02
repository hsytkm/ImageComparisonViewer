using ImageComparisonViewer.Core.Settings;
using ImageComparisonViewer.MainTabControl.ViewModels.Bases;
using Prism.Ioc;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class TabContentSettingsViewModel : TabContentViewModelBase
    {
        private const string _title = "Settings";

        public ReactiveProperty<bool> IsInterlock
        {
            get => _isInterlock;
            private set => SetProperty(ref _isInterlock, value);
        }
        private ReactiveProperty<bool> _isInterlock = default!;

        public TabContentSettingsViewModel(IContainerExtension container) : base(_title)
        {
            Debug.WriteLine($"{nameof(TabContentSettingsViewModel): ctor}");

            var userSettings = container.Resolve<UserSettings>();

            IsInterlock = userSettings
                .ToReactivePropertyAsSynchronized(x => x.IsControlInterlock)
                .AddTo(CompositeDisposable);

        }

    }
}
