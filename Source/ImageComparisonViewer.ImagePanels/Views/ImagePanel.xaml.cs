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
            // VM�Ɉ�����n�������̂Ŏ��O�ŃC���X�^���X���
            DataContext = new ImagePanelViewModel(container, parameter);

            InitializeComponent();

            var parameters = ImageViewParameterFactory.GetImageViewParameters(parameter);
            ExplolerAddressBarControl.Content = container.Resolve<ExplorerAddressBar>(parameters);
            ThumbnailListControl.Content = container.Resolve<ThumbnailList>(parameters);
            ZoomableImageControl.Content = container.Resolve<ZoomableImage>(parameters);

            // ��������
            //ScrollImageViewer.Content = container.Resolve<ScrollImageViewer>(parameters);

            // ����Region�ŊǗ����悤�Ƃ������ǁA�eImagePanel�Ŗ��O���d�����ă_�����ۂ���������
            //var view = container.Resolve<ThumbnailList>();
            //regionManager.Regions["ThumbnailListRegion"].Add(view); //�_��������1

            //regionManager.RegisterViewWithRegion("ThumbnailListRegion", typeof(ThumbnailList)); //�_��������2

            //var view = container.Resolve<ThumbnailList>();
            //regionManager.RegisterViewWithRegion("ThumbnailListRegion", () => view); //�_��������3
            // ����Region�ŊǗ����悤�Ƃ������ǁA�eImagePanel�Ŗ��O���d�����ă_�����ۂ���������
        }

    }
}