using ImageComparisonViewer.Common.Mvvm;
using Prism.Ioc;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ICV.Control.ThumbnailList
{
    /// <summary>
    /// ThumbnailList.xaml の相互作用ロジック
    /// </summary>
    public partial class ThumbnailList : DisposableUserControl
    {
        private readonly IContainerExtension _container;
        private Thumbnails? _thumbnails;

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
                        // PATH内の画像をサムネイルとしてViewに読み出す(Viewクリアのためでもコールする)
                        var paths = (e.NewValue is IReadOnlyList<string> ss) ? ss : null;
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
                        if (!(thumbnailList.TryGetChildControl(out ListBox? listBox))) return;
                        if (!(e.NewValue is string newPath)) return;

                        // 指定された要素を選択する
                        var newThumb = listBox.ItemsSource?.Cast<Thumbnail>()
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

        public ThumbnailList(IContainerExtension container)
        {
            InitializeComponent();

            _container = container;
        }
        
        /// <summary>
        /// 画像リストをViewに読み出す
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="paths">画像のPATHリスト</param>
        private static void SourceImagesPathChanged(object sender, IReadOnlyList<string>? paths)
        {
            if (!(sender is ThumbnailList thumbnailList)) return;
            if (!(thumbnailList.TryGetChildControl(out ListBox? listBox))) return;

            if (paths is null)
            {
                thumbnailList._thumbnails = null;
                listBox.ItemsSource = null;
            }
            else
            {
                var thumbs = new Thumbnails(paths);
                thumbnailList._thumbnails = thumbs;
                listBox.ItemsSource = thumbs.Sources;
            }
        }

        /// <summary>
        /// スクロール変化で表示される画像を読み出し
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            UpdateViewportThumbnail(e.ExtentWidth, e.ViewportWidth, e.HorizontalOffset);
        }

        /// <summary>
        /// 画像コンテンツ切り替え時の画像読み出し
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is ScrollViewer scrollViewer)) return;
            UpdateViewportThumbnail(scrollViewer.ExtentWidth, scrollViewer.ViewportWidth, scrollViewer.HorizontalOffset);
        }

        /// <summary>
        /// Viewに表示されるサムネイルを読み出す
        /// </summary>
        /// <param name="extentWidth"></param>
        /// <param name="viewportWidth"></param>
        /// <param name="horizontalOffset"></param>
        private void UpdateViewportThumbnail(double extentWidth, double viewportWidth, double horizontalOffset)
        {
            static double clip(double v) => (v <= 0) ? 0 : (1 < v ? 1 : v);

            // 表示範囲の中央の割合
            static double getCenterRatio(double length, double viewport, double offset) =>
                (length == 0) ? 0 : clip((offset + (viewport / 2)) / length);

            // 全要素と表示範囲の割合(要素が全て表示されていたら1.0)
            static double getViewportRatio(double length, double viewport) =>
                (length == 0) ? 0 : clip(viewport / length);

            if (_thumbnails is null) return;

            var centerRatio = getCenterRatio(extentWidth, viewportWidth, horizontalOffset);
            var viewportRatio = getViewportRatio(extentWidth, viewportWidth);

            // Viewのサムネイル読み込み状態を更新
            _thumbnails.UpdateThumbnail(centerRatio, viewportRatio);
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
