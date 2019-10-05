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
        private Thumbnails _thumbnails = default!;

        #region SourceImagesPathProperty

        /// <summary>
        /// 表示する画像のPATHリスト
        /// </summary>
        private static readonly DependencyProperty SourceImagesPathProperty =
            DependencyProperty.Register(
                nameof(SourceImagesPath),
                typeof(IReadOnlyList<string>),
                typeof(ThumbnailList),
                new FrameworkPropertyMetadata(
                    default!,
                    FrameworkPropertyMetadataOptions.None,
                    (d, e) =>
                    {
                        // PATHをViewに読み出す
                        if (e.NewValue is IReadOnlyList<string> paths)
                            SourceImagesPathChanged(d, paths);
                    }));

        public IReadOnlyList<string> SourceImagesPath
        {
            get => (IReadOnlyList<string>)GetValue(SourceImagesPathProperty);
            set => SetValue(SourceImagesPathProperty, value);
        }

        #endregion

        #region SelectedImagePathProperty(TwoWay)

        /// <summary>
        /// 選択ファイル(未選択時はnull)
        /// </summary>
        private static readonly DependencyProperty SelectedImagePathProperty =
            DependencyProperty.Register(
                nameof(SelectedImagePath),
                typeof(string),
                typeof(ThumbnailList),
                new FrameworkPropertyMetadata(
                    default,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) =>
                    {
                        if (!(d is ThumbnailList thumbnailList)) return;
                        if (!(ViewHelper.TryGetChildControl(thumbnailList, out ListBox? listBox))) return;
                        if (!(e.NewValue is string newPath)) return;

                        // 指定された要素を選択する
                        var newThumb = listBox.ItemsSource.Cast<Thumbnail>()
                            .FirstOrDefault(x => x.FilePath == newPath);

                        if (listBox.SelectedItem != newThumb)
                            listBox.SelectedItem = newThumb;
                    }));

        public string? SelectedImagePath
        {
            get => (string?)GetValue(SelectedImagePathProperty);
            set => SetValue(SelectedImagePathProperty, value);
        }

        #endregion

        public ThumbnailList()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// 画像リストをViewに読み出す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="paths">画像のPATHリスト</param>
        private static void SourceImagesPathChanged(object sender, IReadOnlyList<string> paths)
        {
            if (!(sender is ThumbnailList thumbnailList)) return;
            if (!(ViewHelper.TryGetChildControl(thumbnailList, out ListBox? listBox))) return;

            var thumbs = new Thumbnails(paths);
            thumbnailList._thumbnails = thumbs;

            listBox.ItemsSource = thumbs.Sources;
        }

        /// <summary>
        /// スクロール変化で表示される画像を読み出し
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

            // Viewのサムネイル読み込み状態を更新
            _thumbnails?.UpdateThumbnail(centerRatio, viewportRatio);
        }

        /// <summary>
        /// ListBoxの選択変更で選択ファイルPATHを更新
        /// </summary>
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListBox listBox)) return;
            if (!(listBox.SelectedItem is Thumbnail thumb)) return;

            if (!(thumb.FilePath is null))
                SelectedImagePath = thumb.FilePath;
        }
    }
}
