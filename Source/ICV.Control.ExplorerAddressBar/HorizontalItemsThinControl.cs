using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ICV.Control.ExplorerAddressBar
{
    /// <summary>
    /// 表示に収まらないItemsSourceの要素をCollapsedにするItemsControl
    /// </summary>
    class HorizontalItemsThinControl : ItemsControl
    {
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
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnParentWidthChanged(e.NewSize.Width);
        }

        private void OnParentWidthChanged(double visibleWidth)
        {
            var elements = this.ItemsSource.Cast<FrameworkElement>().ToArray();
            double sumWidth = 0;

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
