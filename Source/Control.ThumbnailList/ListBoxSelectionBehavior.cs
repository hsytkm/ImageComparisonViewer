using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;

namespace Control.ThumbnailList
{
    class ListBoxSelectionBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        // 選択中アイテムの番号バッファ(選択中アイテムが消されたときの再選択用)
        //private int _selectedIndexBuffer;

        private void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(sender is ListBox listBox)) return;

#if false
            // 選択中アイテムが消されたときの再選択
            if (listBox.SelectedItem == null && listBox.Items.Count > 0)
            {
                static int clip(int v, int max) => (v <= 0) ? 0 : (max < v ? max : v);
                var newIndex = clip(_selectedIndexBuffer, listBox.Items.Count - 1);
                listBox.SelectedItem = listBox.Items[newIndex];
            }
            _selectedIndexBuffer = listBox.SelectedIndex;
#endif

            // 選択項目まで表示をスクロール
            listBox.ScrollIntoView(listBox.SelectedItem);
        }

    }
}
