using ImageComparisonViewer.Common.Mvvm;
using System.Windows.Controls;
using System.Windows.Input;

namespace ICV.Control.ScrollImageViewer.Behaviors
{
    class MouseHorizontalShiftBehavior : BehaviorBase<ScrollViewer>
    {
        protected override void OnLoaded()
        {
            base.OnLoaded();
            AssociatedObject.PreviewMouseWheel += AssociatedObject_PreviewMouseWheel;
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            AssociatedObject.PreviewMouseWheel -= AssociatedObject_PreviewMouseWheel;
        }

        /// <summary>
        /// MouseWheel+ShiftでScrollViewerを左右に移動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void AssociatedObject_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!(sender is ScrollViewer scrollViewer)) return;

            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
            {
                if (e.Delta < 0)
                    scrollViewer.LineRight();
                else
                    scrollViewer.LineLeft();

                e.Handled = true;
            }
        }

    }
}
