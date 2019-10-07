using ICV.Control.ThumbnailList;
using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.ImagePanels.ViewModels;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Linq;

namespace ImageComparisonViewer.ImagePanels.Views
{
    /// <summary>
    /// Interaction logic for ImagePanel.xaml
    /// </summary>
    public partial class ImagePanel : DisposableUserControl
    {
        public ImagePanel(IContainerExtension container, IRegionManager regionManager, int contentIndex, uint contentCount)
        {
            InitializeComponent();

            // VMに引数を渡したいので自前でインスタンス作る
            var viewModel = new ImagePanelViewModel(container, contentIndex, (int)contentCount);
            DataContext = viewModel;

#if false   // ◆メモリリーク怪しいので一旦無効化
            // ◆複数の引数を渡す場合はデータstructに変えましょう
            var parameters = new[]
            {
                (typeof(int), (object)contentIndex),
                (typeof(uint), (object)contentCount),
            };
            var thumbView = container.Resolve<ThumbnailList>(parameters);
            ThumbnailList.Content = thumbView;
#endif

            //var view = container.Resolve<ThumbnailList>();
            //regionManager.Regions["ThumbnailListRegion"].Add(view); //ダメだった1

            //regionManager.RegisterViewWithRegion("ThumbnailListRegion", typeof(ThumbnailList)); //ダメだった2

            //var view = container.Resolve<ThumbnailList>();
            //regionManager.RegisterViewWithRegion("ThumbnailListRegion", () => view); //ダメだった3
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            // 以下がないとメモリリークするっぽい…
            DataContext = null;
        }

    }
}