using ImageComparisonViewer.Common.Mvvm;
using System.Windows;
using System.Windows.Controls;

namespace ICV.Control.ExplorerAddressBar.Behaviors
{
    class VisibilityFocusGetTextBoxBehavior : BehaviorBase<TextBox>
    {
        protected override void OnSetup()
        {
            base.OnSetup();
            AssociatedObject.IsVisibleChanged += AssociatedObject_IsVisibleChanged;
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();
            AssociatedObject.IsVisibleChanged -= AssociatedObject_IsVisibleChanged;
        }

        /// <summary>
        /// Visibleの時にフォーカスを取得する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssociatedObject_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // テキストボックス表示時のフォーカス移行＋カーソル最終文字
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
        }

    }
}
