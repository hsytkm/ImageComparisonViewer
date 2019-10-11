using ImageComparisonViewer.Common.Mvvm;
using ImageComparisonViewer.Core.Images;
using Prism.Commands;
using Prism.Ioc;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace ICV.Control.ExplorerAddressBar
{
    class ExplorerAddressBarViewModel : DisposableBindableBase
    {
        // Modelディレクトリ選択が存在しない場合の初期ディレクトリ
        private static readonly string _defaultDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        #region Visibility
        /// <summary>
        /// DirectoryNodeを表示する(TextBoxを表示しない)
        /// </summary>
        public BooleanNotifier IsVisibleDirectoryNode { get; } = new BooleanNotifier(initialValue: true);

        /// <summary>
        /// TextBoxを表示する(DirectoryNodeを非表示にする)
        /// </summary>
        public DelegateCommand VisibleTextBoxCommand { get; } = default!;

        /// <summary>
        /// TextBoxを非表示にする(DirectoryNodeを表示する)
        /// </summary>
        public DelegateCommand CollapsedTextBoxCommand { get; } = default!;
        #endregion

        /// <summary>
        /// 対象ディレクトリ
        /// </summary>
        public ReactiveProperty<string> TargetDirectory { get; } = default!;

        #region ItemsControl(DirectoryNodes)
        /// <summary>
        /// ディレクトリPATHをModelに通知するAction(子View用)
        /// </summary>
        private readonly Action<string> _sendSerectedDirectoryPathAction;

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
        #endregion

        /// <summary>
        /// TextBox確定時のコマンド
        /// </summary>
        public DelegateCommand<string> TextInputCommand { get; } = default!;

        public ExplorerAddressBarViewModel(IContainerExtension container, ImageViewParameter parameter)
        {
            var compositeDirectory = container.Resolve<CompositeImageDirectory>();
            var imageDirectory = compositeDirectory.ImageDirectries[parameter.ContentIndex];

            #region Visibility

            VisibleTextBoxCommand = new DelegateCommand(() => IsVisibleDirectoryNode.TurnOff());
            CollapsedTextBoxCommand = new DelegateCommand(() => IsVisibleDirectoryNode.TurnOn());

            #endregion

            // Modelのディレクトリ(TwoWay)
            TargetDirectory = imageDirectory
                .ToReactivePropertyAsSynchronized(x => x.DirectoryPath,
                    convert: m => DirectoryNode.EmendFullPathToViewModel((m is null) ? _defaultDirectory : m),
                    convertBack: vm => DirectoryNode.EmendFullPathFromViewModel(vm),
                    mode: ReactivePropertyMode.DistinctUntilChanged)
                .AddTo(CompositeDisposable);

            #region ItemsControl(DirectoryNodes)

            // ディレクトリPATHをModelに通知するAction(子View用)
            _sendSerectedDirectoryPathAction = path => TargetDirectory.Value = path;

            // 親コントロールの幅(制限なし最大)
            UserControlWidth
                .Subscribe(width => UpdateNodesVisibility(width, ItemsControlWidth.Value))
                .AddTo(CompositeDisposable);

            // 自コントロールの幅(制限あり)
            ItemsControlWidth
                .Subscribe(width => UpdateNodesVisibility(UserControlWidth.Value, width))
                .AddTo(CompositeDisposable);

            // Modelのディレクトリを購読
            var targetDirectory = imageDirectory
                .ObserveProperty(x => x.DirectoryPath)
                .ToReadOnlyReactiveProperty(mode: ReactivePropertyMode.Default)
                .AddTo(CompositeDisposable);

            // ディレクトリViewの読込み
            targetDirectory
                .Subscribe(path => UpdateViewsSource(path is null ? _defaultDirectory : path))
                .AddTo(CompositeDisposable);

            #endregion

            // TextBox入力確定時のディレクトリ通知
            TextInputCommand = new DelegateCommand<string>(path =>
            {
                if (Directory.Exists(path))
                    TargetDirectory.Value = path;

                IsVisibleDirectoryNode.TurnOn();
            });

        }

        #region UpdateNodesVisibility
        // <summary>
        // NodeBarの子要素と積み上げ幅の逆順リスト(逆= 末端ディレクトリが先頭)
        // FrameworkElementのVisibilityをCollapsedにするとサイズが取得できなくなるので、
        // 表示中にサイズを保持する
        // </summary>
        private readonly IList<(FrameworkElement Element, double SumWidth)> _fwElementWidths =
            new List<(FrameworkElement Element, double SumWidth)>();

        /// <summary>
        /// コントロールのサイズに応じてDirectoryNodeViewのVisibilityを切り替える
        /// </summary>
        /// <param name="visibleWidth"></param>
        /// <param name="unlimitedWidth"></param>
        private void UpdateNodesVisibility(double visibleWidth, double unlimitedWidth)
        {
            //Debug.WriteLine($"UpdateNodesVisibility Width: {visibleWidth:f2} / {unlimitedWidth:f2}");
            if (visibleWidth == 0 || unlimitedWidth == 0) return;

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
        #endregion

        #region UpdateViewsSource
        /// <summary>
        /// Viewのディレクトリノードを更新
        /// </summary>
        /// <param name="sourcePath"></param>
        private void UpdateViewsSource(string? sourcePath = null)
        {
            if (ViewItemsSource.Value != null)
            {
                foreach (var item in ViewItemsSource.Value)
                {
                    if (item is IDisposable disposable)
                        disposable.Dispose();
                }
                ViewItemsSource.Value.Clear();
            }

            if (!string.IsNullOrEmpty(sourcePath))
            {
                ViewItemsSource.Value = DirectoryNode.GetDirectoryNodes(sourcePath)
                    .Select(node => new DirectoryPathNode(node, _sendSerectedDirectoryPathAction))
                    .ToList();
            }
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            // ディレクトリなしで要求してコントロールを削除する!!
            UpdateViewsSource();

            base.Dispose(disposing);
        }

    }
}
