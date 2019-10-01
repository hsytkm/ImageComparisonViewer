using Control.ImagePanel.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Control.ImagePanel.Views
{
    /// <summary>
    /// Interaction logic for ImagePanel.xaml
    /// </summary>
    public partial class ImagePanel : UserControl
    {
        #region ContentIndexProperty

        // 画像コンテンツ番号
        private static readonly DependencyProperty ContentIndexProperty =
            DependencyProperty.Register(
                nameof(ContentIndex),
                typeof(int),
                typeof(ImagePanel),
                new FrameworkPropertyMetadata(
                    -1,
                    FrameworkPropertyMetadataOptions.None,
                    (d, e) =>
                    {
                        if (d is ImagePanel view && view.DataContext is ImagePanelViewModel vmodel)
                        {
                            if (e.NewValue is int index)
                                vmodel.ContentIndex = index;
                        }
                    }));

        public int ContentIndex
        {
            get => (int)GetValue(ContentIndexProperty);
            set => SetValue(ContentIndexProperty, value);
        }

        #endregion

        #region IsActiveProperty

        // ViewのActiveフラグ(TabControlから伝搬)
        private static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register(
                nameof(IsActive),
                typeof(bool),
                typeof(ImagePanel),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.None,
                    (d, e) =>
                    {
                        if (d is ImagePanel view && view.DataContext is ImagePanelViewModel vmodel)
                        {
                            if (e.NewValue is bool isActive)
                                vmodel.IsActive.Value = isActive;
                        }
                    }));

        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        #endregion

        public ImagePanel()
        {
            InitializeComponent();
        }

    }
}