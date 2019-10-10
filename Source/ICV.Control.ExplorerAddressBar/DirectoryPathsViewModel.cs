using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core.Images;
using Prism.Commands;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;

namespace ICV.Control.ExplorerAddressBar
{
    class DirectoryPathsViewModel : DisposableBindableBase
    {
        private static readonly string _defaultDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        /// <summary>
        /// 親コントロールの幅(制限なし最大)
        /// </summary>
        public ReactiveProperty<double> UserControlWidth { get; } =
            new ReactiveProperty<double>(mode: ReactivePropertyMode.DistinctUntilChanged);

        /// <summary>
        /// 自コントロールの幅(制限あり)
        /// </summary>
        public ReactiveProperty<double> ItemsControlWidth { get; } =
            new ReactiveProperty<double>(mode: ReactivePropertyMode.DistinctUntilChanged);

        /// <summary>
        /// View(ディレクトリノード)のリスト
        /// </summary>
        public ReactiveProperty<IList<DirectoryPathNode>> ViewItemsSource { get; } =
             new ReactiveProperty<IList<DirectoryPathNode>>(mode: ReactivePropertyMode.None);

        /// <summary>
        /// ディレクトリPATHをModelに通知するAction(子View用)
        /// </summary>
        private readonly Action<string> _sendSerectedDirectoryPathAction;

        public DirectoryPathsViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            var compositeDirectory = container.Resolve<CompositeImageDirectory>();
            var imageDirectory = compositeDirectory.ImageDirectries[parameter.ContentIndex];

            _sendSerectedDirectoryPathAction = path => imageDirectory.DirectoryPath = path;

            // Modelのディレクトリを購読
            var targetDirectory = imageDirectory
                .ObserveProperty(x => x.DirectoryPath)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.Default)
                .AddTo(CompositeDisposable);

            targetDirectory
                .Subscribe(path => AddViewNodes(path is null ? _defaultDirectory : path))
                .AddTo(CompositeDisposable);

            // 親コントロールの幅(制限なし最大)
            UserControlWidth
                .Subscribe(width => UpdateNodesVisibility(width, ItemsControlWidth.Value))
                .AddTo(CompositeDisposable);

            // 自コントロールの幅(制限あり)
            ItemsControlWidth
                .Subscribe(width => UpdateNodesVisibility(UserControlWidth.Value, width))
                .AddTo(CompositeDisposable);
        }

        // <summary>
        // NodeBarの子要素と積み上げ幅の逆順リスト(逆= 末端ディレクトリが先頭)
        // FrameworkElementのVisibilityをCollapsedにするとサイズが取得できなくなるので、
        // 表示中にサイズを保持する
        // </summary>
        private readonly IList<(FrameworkElement Element, double SumWidth)> _fwElementWidths =
            new List<(FrameworkElement Element, double SumWidth)>();

        private void UpdateNodesVisibility(double visibleWidth, double unlimitedWidth)
        {
            if (visibleWidth == 0 || unlimitedWidth == 0) return;
            Debug.WriteLine($"DirectoryPathBar Width: {visibleWidth:f2} / {unlimitedWidth:f2}");

            // ItemsControlの子要素達
            var sources = ViewItemsSource.Value;

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
                    //Dispose(_fwElementWidths.Select(x => x.Element));
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

        /// <summary>
        /// Viewのディレクトリノードを更新
        /// </summary>
        /// <param name="eab"></param>
        /// <param name="sourcePath"></param>
        private void AddViewNodes(string? sourcePath)
        {
            if (ViewItemsSource.Value != null)
            {
                Dispose(ViewItemsSource.Value);
                ViewItemsSource.Value.Clear();
            }

            if (!string.IsNullOrEmpty(sourcePath))
            {
                var path = DirectoryNode.EmendFullPath(sourcePath);
                var views = new List<DirectoryPathNode>();

                // ノードViewを順に作成してRegionに登録
                foreach (var directoryNode in DirectoryNode.GetDirectoryNodes(path))
                {
                    views.Add(new DirectoryPathNode(directoryNode, _sendSerectedDirectoryPathAction));
                }

                ViewItemsSource.Value = views;
            }
        }

        private static void Dispose<T>(IEnumerable<T> source) where T : class
        {
            if (source is null) return;
            foreach (var obj in source)
            {
                if (obj is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        protected override void Dispose(bool disposing)
        {
            Dispose(ViewItemsSource.Value);
            ViewItemsSource.Value.Clear();

            // ◆いる？未確認
            //Dispose(_fwElementWidths.Select(x => x.Element));
            //_fwElementWidths.Clear();

            base.Dispose(disposing);
        }

    }
}
