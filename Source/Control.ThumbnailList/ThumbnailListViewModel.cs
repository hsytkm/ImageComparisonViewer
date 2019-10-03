using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;

namespace Control.ThumbnailList
{
#if false
    class ThumbnailListViewModel : BindableBase
    {
        //public ReadOnlyObservableCollection<ThubnailVModel> Thumbnails { get; }

        //public ReactiveProperty<ThubnailVModel> SelectedItem { get; }

        // スクロール変化時
        //public ReactiveProperty<(double CenterRatio, double ViewportRatio)> ScrollChangedHorizontal { get; } =
        //    new ReactiveProperty<(double CenterRatio, double ViewportRatio)>(mode: ReactivePropertyMode.DistinctUntilChanged);
        
        public ThumbnailListViewModel(IContainerExtension container)
        {
            //var imageSources = container.Resolve<ImageSources>();
            //var modelImageSources = imageSources.Sources;

            //// Mの画像リストをVM用に変換(schedulerがないと意図通りに動作しないけど理解できていない…)
            //Thumbnails = modelImageSources
            //    .ToReadOnlyReactiveCollection(m => new ThubnailVModel(m), scheduler: Scheduler.CurrentThread);

            //// Mの画像更新をVMに通知(解放時のnullも通知される)
            //modelImageSources
            //    .ObserveElementProperty(m => m.Thumbnail)
            //    .Subscribe(m =>
            //    {
            //        //Console.WriteLine($"{x.Instance} {x.Property} {x.Value}");
            //        var vmItem = Thumbnails.FirstOrDefault(vm => vm.FilePath == m.Instance.FilePath);
            //        if (vmItem != null) vmItem.Image = m.Value;
            //    });

            // VM⇔Mの通知を以下に置き換えた(Thumbnailsのscheduler指定がポイント)
            //SelectedItem = imageSources
            //    .ToReactivePropertyAsSynchronized(x => x.SelectedImagePath,
            //        // M->VM
            //        convert: m => Thumbnails.FirstOrDefault(vm => vm.FilePath == m),
            //        // M<-VM
            //        convertBack: vm => imageSources.SelectedImagePath = vm?.FilePath);

            // スクロール操作時の画像読出/解放
            //ScrollChangedHorizontal
            //    .Subscribe(x => imageSources.UpdateThumbnail(x.CenterRatio, x.ViewportRatio));

        }
    }

    class ThubnailVModel : BindableBase
    {
        public BitmapSource? Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
        private BitmapSource? _image;
        public string? FilePath { get; }
        public string? Filename { get; }

        public ThubnailVModel(ImageSource source)
        {
            Image = source?.Thumbnail;
            FilePath = source?.FilePath;
            Filename = source?.Filename;
        }
    }
#endif

}
