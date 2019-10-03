using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Control.ThumbnailList
{
    /// <summary>
    /// ThumbnailList.xaml の相互作用ロジック
    /// </summary>
    public partial class ThumbnailList : UserControl
    {
        #region SourceImagesPathProperty

        /// <summary>
        /// 表示する画像のPATHリスト
        /// </summary>
        private static readonly DependencyProperty SourceImagesPathProperty =
            DependencyProperty.Register(
                nameof(SourceImagesPath),
                typeof(IReadOnlyCollection<string>),
                typeof(ThumbnailList),
                new FrameworkPropertyMetadata(
                    default!,
                    FrameworkPropertyMetadataOptions.None,
                    (d, e) =>
                    {
                        // PATHをViewに読み出す
                        if (e.NewValue is IReadOnlyCollection<string> paths)
                            SourceImagesPathChanged(d, paths);
                    }));

        public IReadOnlyCollection<string> SourceImagesPath
        {
            get => (IReadOnlyCollection<string>)GetValue(SourceImagesPathProperty);
            set => SetValue(SourceImagesPathProperty, value);
        }

        #endregion

        #region SelectedImagePathProperty(TwoWay)

        /// <summary>
        /// 選択ディレクトリ
        /// </summary>
        private static readonly DependencyProperty SelectedImagePathProperty =
            DependencyProperty.Register(
                nameof(SelectedImagePath),
                typeof(string),
                typeof(ThumbnailList),
                new FrameworkPropertyMetadata(
                    default!,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) =>
                    {
                    }));

        public string SelectedImagePath
        {
            get => (string)GetValue(SelectedImagePathProperty);
            set => SetValue(SelectedImagePathProperty, value);
        }

        #endregion

        public ThumbnailList()
        {
            InitializeComponent();
        }

        private static void SourceImagesPathChanged(object sender, IReadOnlyCollection<string> paths)
        {
            if (!(sender is ThumbnailList thumbnailList)) return;
            if (!(ViewHelper.TryGetChildControl(thumbnailList, out ListBox? listBox))) return;

            var thumbs = paths.Select(x => new ThubnailVModel(new ImageSource(x)));

            listBox.ItemsSource = thumbs;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            static double clip(double v) => (v <= 0) ? 0 : (1 < v ? 1 : v);

            // 表示範囲の中央の割合
            static double getCenterRatio(double length, double viewport, double offset) =>
                (length == 0) ? 0 : clip((offset + (viewport / 2)) / length);

            // 全要素と表示範囲の割合(要素が全て表示されていたら1.0)
            static double getViewportRatio(double length, double viewport) =>
                (length == 0) ? 0 : clip(viewport / length);

            var centerRatio = getCenterRatio(e.ExtentWidth, e.ViewportWidth, e.HorizontalOffset);
            var viewportRatio = getViewportRatio(e.ExtentWidth, e.ViewportWidth);


            //imageSources.UpdateThumbnail(centerRatio, viewportRatio);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListBox listBox)) return;
            if (!(listBox.SelectedItem is ThubnailVModel thubnail)) return;

            if (!(thubnail.FilePath is null))
            {
                SelectedImagePath = thubnail.FilePath;
            }
        }
    }
}
