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
        #region ContentIndexProperty(コンストラクタ渡しに変更して不使用)

        // 画像コンテンツ番号
        //private static readonly DependencyProperty ContentIndexProperty =
        //    DependencyProperty.Register(
        //        nameof(ContentIndex),
        //        typeof(int),
        //        typeof(ImagePanel),
        //        new FrameworkPropertyMetadata(
        //            -1,
        //            FrameworkPropertyMetadataOptions.None,
        //            (d, e) =>
        //            {
        //                if (d is ImagePanel view && view.DataContext is ImagePanelViewModel vmodel)
        //                {
        //                    if (e.NewValue is int index)
        //                        vmodel.ContentIndex = index;
        //                }
        //            }));

        //public int ContentIndex
        //{
        //    get => (int)GetValue(ContentIndexProperty);
        //    set => SetValue(ContentIndexProperty, value);
        //}

        #endregion

        #region IsActiveProperty(Load/Unloadイベントで管理する方式に変更して不使用)

        // ViewのActiveフラグ(TabControlから伝搬)
        //private static readonly DependencyProperty IsActiveProperty =
        //    DependencyProperty.Register(
        //        nameof(IsActive),
        //        typeof(bool),
        //        typeof(ImagePanel),
        //        new FrameworkPropertyMetadata(
        //            false,
        //            FrameworkPropertyMetadataOptions.None,
        //            (d, e) =>
        //            {
        //                if (d is ImagePanel view && view.DataContext is ImagePanelViewModel vmodel)
        //                {
        //                    if (e.NewValue is bool isActive)
        //                    {
        //                        vmodel.IsActive.Value = isActive;
        //                    }
        //                }
        //            }));

        //public bool IsActive
        //{
        //    get => (bool)GetValue(IsActiveProperty);
        //    set => SetValue(IsActiveProperty, value);
        //}

        #endregion

        public ImagePanel(IContainerExtension container, IRegionManager regionManager, int contentIndex, uint contentLength)
        {
            InitializeComponent();

            // VMに引数を渡したいので自前でインスタンス作る
            var viewModel = new ImagePanelViewModel(container, contentIndex, (int)contentLength);
            DataContext = viewModel;

            var thumbView = container.Resolve<ThumbnailList>();
            ThumbnailList.Content = thumbView;

            //var view = container.Resolve<ThumbnailList>();
            //regionManager.Regions["ThumbnailListRegion"].Add(view); //ダメだった1

            //regionManager.RegisterViewWithRegion("ThumbnailListRegion", typeof(ThumbnailList)); //ダメだった2

            //var view = container.Resolve<ThumbnailList>();
            //regionManager.RegisterViewWithRegion("ThumbnailListRegion", () => view); //ダメだった3
        }

    }
}