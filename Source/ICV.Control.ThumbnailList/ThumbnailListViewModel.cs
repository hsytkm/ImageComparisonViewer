using ImageComparisonViewer.Core;
using Prism.Ioc;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ICV.Control.ThumbnailList
{
    class ThumbnailListViewModel : BindableBase
    {
        private readonly IContainerExtension _container;

        //public ReadOnlyObservableCollection<string> Thumbnails { get; }

        public ReactiveProperty<string> SelectedItem
        {
            get => _selectedItem;
            private set => SetProperty(ref _selectedItem, value);
        }
        private ReactiveProperty<string> _selectedItem = default!;

        //public ReactiveProperty<(double CenterRatio, double ViewportRatio)> ScrollChangedHorizontal { get; } =
        //    new ReactiveProperty<(double CenterRatio, double ViewportRatio)>(mode: ReactivePropertyMode.DistinctUntilChanged);

        public ThumbnailListViewModel(IContainerExtension container)
        {
            _container = container;

        }

        public void Load()
        {
            var imageSources = _container.Resolve<ImageSources>();

            SelectedItem
                .Subscribe(x => Debug.WriteLine(x));

        }

        public void Unload()
        {

        }

    }
}
