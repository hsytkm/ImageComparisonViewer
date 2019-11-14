using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ICV.Control.ExplorerAddressBar
{
    /// <summary>
    /// 表示に収まらないItemsSourceの要素をCollapsedにするItemsControl
    /// </summary>
    class HorizontalItemsThinControl : ItemsControl
    {
        // バー右側の非表示領域の幅
        private const double _marginWidth = 25;

        // ItemsSourceの最小表示数
        public int VisibleItemMin
        {
            get => (int)GetValue(VisibleItemMinProperty);
            set => SetValue(VisibleItemMinProperty, value);
        }
        private static readonly DependencyProperty VisibleItemMinProperty =
            DependencyProperty.Register(nameof(VisibleItemMin), typeof(int), typeof(HorizontalItemsThinControl));

        public HorizontalItemsThinControl()
        {
            SizeChanged += ItemsControl_OnSizeChanged;

            // ItemsSource変化時の再描画
            //  <ItemsControl.ItemsPanel>
            //      <ItemsPanelTemplate>
            //          <StackPanel Orientation="Horizontal" />
            //      </ItemsPanelTemplate>
            //  </ItemsControl.ItemsPanel>
            var factoryPanel = new FrameworkElementFactory(typeof(StackPanel));
            factoryPanel.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            factoryPanel.AddHandler(FrameworkElement.SizeChangedEvent, new SizeChangedEventHandler(ChildPanel_OnSizeChanged));
            this.ItemsPanel = new ItemsPanelTemplate { VisualTree = factoryPanel };
        }

        private void ItemsControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateItemsSourceVisibility(e.NewSize.Width);
        }

        private void ChildPanel_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent((DependencyObject)sender);
            UpdateItemsSourceVisibility(((FrameworkElement)parent).ActualWidth);
        }

        private void UpdateItemsSourceVisibility(double visibleWidth)
        {
            var elements = this.ItemsSource.Cast<FrameworkElement>().ToArray();
            double sumWidth = _marginWidth;   // 余白分を確保しておく

            for (int counter = 1; counter <= elements.Length; counter++)
            {
                var element = elements[^counter];   // 後ろからn番目

                // サイズ取得のため一回表示させる
                element.Visibility = Visibility.Visible;
                sumWidth += element.ActualWidth;

                if (counter > this.VisibleItemMin && sumWidth > visibleWidth)
                {
                    element.Visibility = Visibility.Collapsed;
                }
            }
        }

    }
}
