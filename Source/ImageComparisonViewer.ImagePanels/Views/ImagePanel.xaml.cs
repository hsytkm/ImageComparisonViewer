using ICV.Control.ExplorerAddressBar;
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
        public ImagePanel(IContainerExtension container, IRegionManager regionManager, ImageViewParameter parameter)
        {
            // VMに引数を渡したいので自前でインスタンス作る
            DataContext = new ImagePanelViewModel(container, parameter);

            InitializeComponent();

            var parameters = ImageViewParameterFactory.GetImageViewParameters(parameter);
            ExplolerAddressBar.Content = container.Resolve<ExplorerAddressBar>(parameters);
            ThumbnailList.Content = container.Resolve<ThumbnailList>(parameters);

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