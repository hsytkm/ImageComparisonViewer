using ICV.Control.ExplorerAddressBar;
using ICV.Control.ScrollImageViewer;
using ICV.Control.ThumbnailList;
using ICV.Control.ZoomableImage.Views;
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
        public ImagePanel(IContainerExtension container, IRegionManager regionManager, ImageViewParameter parameter)
        {
            // VMに引数を渡したいので自前でインスタンス作る
            DataContext = new ImagePanelViewModel(container, parameter);

            InitializeComponent();

            var parameters = ImageViewParameterFactory.GetImageViewParameters(parameter);
            ExplolerAddressBarControl.Content = container.Resolve<ExplorerAddressBar>(parameters);
            ThumbnailListControl.Content = container.Resolve<ThumbnailList>(parameters);
            ZoomableImageControl.Content = container.Resolve<ZoomableImage>(parameters);

            // ◆実装中
            //ScrollImageViewer.Content = container.Resolve<ScrollImageViewer>(parameters);

            // ↓↓Regionで管理しようとしたけど、各ImagePanelで名前が重複してダメっぽかった↓↓
            //var view = container.Resolve<ThumbnailList>();
            //regionManager.Regions["ThumbnailListRegion"].Add(view); //ダメだった1

            //regionManager.RegisterViewWithRegion("ThumbnailListRegion", typeof(ThumbnailList)); //ダメだった2

            //var view = container.Resolve<ThumbnailList>();
            //regionManager.RegisterViewWithRegion("ThumbnailListRegion", () => view); //ダメだった3
            // ↑↑Regionで管理しようとしたけど、各ImagePanelで名前が重複してダメっぽかった↑↑
        }

    }
}