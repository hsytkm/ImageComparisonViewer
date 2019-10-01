using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Control.ExplorerAddressBar
{
    /// <summary>
    /// Interaction logic for ExplorerAddressBar.xaml
    /// </summary>
    public partial class ExplorerAddressBar : UserControl
    {
        private static readonly string _defaultDirectory = default!;
            //Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

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
                typeof(ExplorerAddressBar),
                new FrameworkPropertyMetadata(
                    _defaultDirectory,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) =>
                    {
                        if (!(d is ExplorerAddressBar eab)) return;

                        if (e.NewValue is null)
                        {
                            // Viewをクリアするためにコールする
                            AddViewNodes(eab, sourcePath: "");
                        }
                        else if (e.NewValue is string path)
                        {
                            AddViewNodes(eab, path);
                        }
                    }));

        public string SelectedDirectory
        {
            get => (string)GetValue(SelectedDirectoryProperty);
            set => SetValue(SelectedDirectoryProperty, value);
        }

        #endregion

        public ExplorerAddressBar()
        {
            InitializeComponent();

            #region NodeBar子要素の表示切替

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

            #endregion

            #region DirectoryPathTextBox Events

            // 初回のテキストボックス非表示
            DirectoryPathTextBox.Loaded += (sender, _) =>
            {
                if (sender is TextBox textBox)
                {
                    textBox.Text = _defaultDirectory;
                    SetTextBoxVisibility(false);
                }
            };

            // テキストボックス表示時のフォーカス移行＋カーソル最終文字
            DirectoryPathTextBox.IsVisibleChanged += (sender, e) =>
            {
                if (sender is TextBox textBox && e.NewValue is bool b && b)
                {
                    // Focus()をコール瞬間に、該当項目が !IsEnabled だとフォーカスが移動しないので遅延させる
                    // https://rksoftware.wordpress.com/2016/06/03/001-8/
                    this.Dispatcher.InvokeAsync(() =>
                    {
                        textBox.Focus();
                        textBox.Select(textBox.Text.Length, 0);
                    });
                }
            };

            #endregion

            #region テキストボックスの外領域クリックで非表示化

            // テキストボックスの外領域クリックで非表示化
            DirectoryPathTextBox.MouseDown += (_, e) => e.Handled = true;

            this.Loaded += (_, __) =>
            {
                // テキストボックスの外領域クリックで非表示化
                var window = Window.GetWindow(this);
                window.MouseDown += (_, __) =>
                {
                    SetTextBoxVisibility(false);
                };
            };

            #endregion

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
        private static void AddViewNodes(ExplorerAddressBar eab, string sourcePath)
        {
            if (!ViewHelper.TryGetChildControl<ItemsControl>(eab, out var itemsControl))
                return;

            // 無効文字ならViewをクリア
            if (string.IsNullOrEmpty(sourcePath))
            {
                itemsControl.ItemsSource = null;
                eab.SelectedDirectory = "";
            }
            else
            {
                var path = DirectoryNode.EmendFullPath(sourcePath);
                var views = new List<DirectoryPathNode>();

                // Viewを順に作成してRegionに登録
                foreach (var directoryNode in DirectoryNode.GetDirectoryNodes(path))
                {
                    views.Add(new DirectoryPathNode(directoryNode, path => AddViewNodes(eab, path)));
                }

                itemsControl.ItemsSource = views;
                eab.SelectedDirectory = path;
            }
        }

        /// <summary>
        /// バーの余白部クリックで入力用のテキストボックスを表示する
        /// </summary>
        private void GroundMargin_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
            SetTextBoxVisibility(true);

        /// <summary>
        /// テキストボックスの表示切り替え(Property <-> TextBox)
        /// </summary>
        private void SetTextBoxVisibility(bool isVisible)
        {
            // テキストボックスの表示文字列は、表示時に設定/非表示時にクリア
            if (isVisible)
            {
                // Property -> TextBox
                if (DirectoryPathTextBox.Visibility != Visibility.Visible)
                {
                    DirectoryPathTextBox.Visibility = Visibility.Visible;
                    DirectoryPathTextBox.Text = SelectedDirectory;
                }
            }
            else
            {
                // TextBox -> Property
                if (DirectoryPathTextBox.Visibility != Visibility.Collapsed)
                {
                    DirectoryPathTextBox.Visibility = Visibility.Collapsed;

                    // 入力のディレクトリが存在したら採用
                    var path = DirectoryNode.EmendFullPath(DirectoryPathTextBox.Text);
                    if (Directory.Exists(path))
                        AddViewNodes(this, path);
                }
            }
        }

        /// <summary>
        /// Enterキー押下でテキストの入力PATHを確定させる(TextBox -> Property)
        /// </summary>
        private void DirectoryPathTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Enter))
                SetTextBoxVisibility(false);
        }

    }
}
