using ImageComparisonViewer.Common.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ICV.Control.ExplorerAddressBar
{
    /// <summary>
    /// DirectoryPaths.xaml の相互作用ロジック
    /// </summary>
    public partial class DirectoryPaths : DisposableUserControl
    {
        //private static readonly string _defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        private double _nodeBarVisibleWidth;    // NodeBarの表示幅
        private double _nodeBarUnlimitedWidth;  // NodeBarの制限なし幅(理想幅)

        /// <summary>
        /// NodeBarの子要素と積み上げ幅の逆順リスト(逆=末端ディレクトリが先頭)
        /// FrameworkElementのVisibilityをCollapsedにするとサイズが取得できなくなるので、
        /// 表示中にサイズを保持する
        /// </summary>
        private readonly IList<(FrameworkElement Element, double SumWidth)> _fwElementWidths =
            new List<(FrameworkElement Element, double SumWidth)>();

        #region SelectedDirectoryProperty(TwoWay)

        // 選択ディレクトリ
        private static readonly DependencyProperty SelectedDirectoryProperty =
            DependencyProperty.Register(
                nameof(SelectedDirectory),
                typeof(string),
                typeof(DirectoryPaths),
                new FrameworkPropertyMetadata(
                    default!,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) =>
                    {
                        if (!(d is DirectoryPaths dirView)) return;

                        // ViewをクリアするためPATHが無効でもコールする
                        string sourcePath = (e.NewValue is string path) ? path : "";
                        AddViewNodes(dirView, sourcePath);
                    }));

        public string SelectedDirectory
        {
            get => (string)GetValue(SelectedDirectoryProperty);
            set => SetValue(SelectedDirectoryProperty, value);
        }

        #endregion

        public DirectoryPaths()
        {
            InitializeComponent();

            // 選択ディレクトリでViewを更新
            this.Loaded += (_, __) =>
            {
                AddViewNodes(this, SelectedDirectory);
            };

            // NodeBarの表示幅
            this.SizeChanged += (_, e) =>
            {
                _nodeBarVisibleWidth = e.NewSize.Width;
                UpdateNodesVisibility();
            };

            // NodeBarの制限なし幅
            NodeItemsControl.SizeChanged += (_, e) =>
            {
                _nodeBarUnlimitedWidth = e.NewSize.Width;
                UpdateNodesVisibility();
            };
        }

        #region UpdateNodesVisibility

        // アドレスバーのNode(ディレクトリ)の表示を切り替える
        private void UpdateNodesVisibility()
        {
            double visibleWidth = _nodeBarVisibleWidth;
            double unlimitedWidth = _nodeBarUnlimitedWidth;
            if (visibleWidth == 0 || unlimitedWidth == 0) return;

            //System.Diagnostics.Debug.WriteLine($"DirectoryPathBar Width: {visibleWidth:f2} / {unlimitedWidth:f2}");

            // ItemsControlの子要素達
            var sources = GetItemsControlSources(NodeItemsControl);

            // 全コントロールがVisibleの時点でバッファする
            var feWidths = buffItemSourcesWidths(sources);

            // 表示の余白幅(正数なら表示させる方向)
            if (visibleWidth - unlimitedWidth < 0)
            {
                // 表示幅が狭まったので非表示化する
                ToCollapseNodes(feWidths, visibleWidth);
            }
            else
            {
                // 表示幅が広がったので再表示化する
                ToVisibleNodes(feWidths, visibleWidth);
            }

            // 全コントロールがVisibleの時点でバッファする
            IList<(FrameworkElement Element, double SumWidth)>
                buffItemSourcesWidths(IEnumerable<FrameworkElement> elements)
            {
                if (elements.All(x => x.Visibility == Visibility.Visible))
                {
                    foreach (var fe in _fwElementWidths.Select(x => x.Element))
                    {
                        if (fe is IDisposable disposable)
                            disposable.Dispose();
                    }
                    _fwElementWidths.Clear();

                    // リストは逆管理
                    double sum = 0;
                    foreach (var element in elements.Reverse())
                    {
                        sum += element.ActualWidth;
                        _fwElementWidths.Add((element, sum));
                    }
                }
                return _fwElementWidths;
            }

            // 表示幅が狭まったので非表示化する
            static void ToCollapseNodes(IList<(FrameworkElement Element, double SumWidth)> ews, double viewWidth)
            {
                // 最小でも2つは表示させる(現＋上ディレクトリ)
                foreach (var (Element, SumWidth) in ews.Skip(2))
                {
                    if (SumWidth > viewWidth)
                        Element.Visibility = Visibility.Collapsed;
                }
            }

            // 表示幅が広がったので再表示化する
            static void ToVisibleNodes(IList<(FrameworkElement Element, double SumWidth)> ews, double viewWidth)
            {
                foreach (var (Element, SumWidth) in ews)
                {
                    if (Element.Visibility != Visibility.Visible)
                    {
                        if (SumWidth < viewWidth)
                            Element.Visibility = Visibility.Visible;
                        break;  // 1つ判定したら終わり
                    }
                }
            }
        }

        // ItemsControl内の子要素を取得する(DirectoryPathNode)
        private static IEnumerable<FrameworkElement> GetItemsControlSources(ItemsControl itemsControl)
        {
            if (itemsControl?.ItemsSource is null) yield break;

            foreach (var item in itemsControl.ItemsSource)
            {
                if (item is FrameworkElement fe)
                    yield return fe;
            }
        }

        #endregion

        /// <summary>
        /// Viewのディレクトリノードを更新
        /// </summary>
        /// <param name="eab"></param>
        /// <param name="sourcePath"></param>
        private static void AddViewNodes(DirectoryPaths dirView, string sourcePath)
        {
            if (!dirView.TryGetChildControl<ItemsControl>(out var itemsControl))
                return;

            // 無効文字ならViewをクリア
            if (string.IsNullOrEmpty(sourcePath))
            {
                if (itemsControl.ItemsSource != null)
                {
                    foreach (var obj in itemsControl.ItemsSource)
                    {
                        if (obj is IDisposable disposable)
                            disposable.Dispose();
                    }
                }
                itemsControl.ItemsSource = null;
                dirView.SelectedDirectory = "";
            }
            else
            {
                var path = DirectoryNode.EmendFullPath(sourcePath);
                var views = new List<DirectoryPathNode>();

                // ノードViewを順に作成してRegionに登録
                foreach (var directoryNode in DirectoryNode.GetDirectoryNodes(path))
                {
                    //var view = new DirectoryPathNode(directoryNode, path => AddViewNodes(dirView, path));
                    var view = new DirectoryPathNode(directoryNode);
                    views.Add(view);
                }

                itemsControl.ItemsSource = views;
                dirView.SelectedDirectory = path;
            }
        }

    }
}
