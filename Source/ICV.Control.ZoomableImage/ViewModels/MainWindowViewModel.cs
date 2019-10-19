using ICV.Control.ZoomableImage.Models;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using System;

namespace ICV.Control.ZoomableImage.ViewModels
{
    class MainWindowViewModel : BindableBase
    {
        //public ReactiveProperty<bool> IsImageViewerInterlock { get; } = new ReactiveProperty<bool>(true);
        //public ReactiveProperty<bool> CanVisibleReducedImage { get; } = new ReactiveProperty<bool>(true);
        //public ReactiveProperty<bool> IsVisibleImageOverlapSamplingFrame { get; } = new ReactiveProperty<bool>(true);
        //public ReactiveProperty<bool> IsVisibleSamplingFrameOnImage { get; } = new ReactiveProperty<bool>(true);

        //public MainWindowViewModel(IContainerExtension container, IRegionManager regionManager)
        //{
        //    var viewSettings = container.Resolve<ViewSettings>();

        //    IsImageViewerInterlock.Subscribe(x => viewSettings.IsImageViewerInterlock = x);
        //    CanVisibleReducedImage.Subscribe(x => viewSettings.CanVisibleReducedImage = x);
        //    IsVisibleImageOverlapSamplingFrame.Subscribe(x => viewSettings.IsVisibleImageOverlapSamplingFrame = x);
        //    IsVisibleSamplingFrameOnImage.Subscribe(x => viewSettings.IsVisibleSamplingFrameOnImage = x);

        //}

    }
}
