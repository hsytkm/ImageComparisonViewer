using ImageComparisonViewer.Common.Mvvm;
using System.Windows.Controls;

namespace ICV.Control.ThumbnailList.Behavior
{
    class ListBoxSelectionBehavior : BehaviorBase<ListBox>
    {
        protected override void OnLoaded()
        {
            base.OnLoaded();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;

            // x画面切り替えだとLoaded時点で画像選択されていることがあるので移動する
            ScrollIntoView(AssociatedObject);
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private static void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListBox listBox)) return;

            ScrollIntoView(listBox);
        }

        // 選択項目まで表示をスクロール
        private static void ScrollIntoView(ListBox listBox)
        {
            listBox.ScrollIntoView(listBox.SelectedItem);
        }

    }
}
