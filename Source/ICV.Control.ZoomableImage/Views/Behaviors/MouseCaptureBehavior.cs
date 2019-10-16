using ImageComparisonViewer.Common.Mvvm;
using System.Windows;
using System.Windows.Input;

namespace ICV.Control.ZoomableImage.Views.Behaviors
{
    class MouseCaptureBehavior : BehaviorBase<FrameworkElement>
    {
        protected override void OnLoaded()
        {
            base.OnLoaded();
            AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseButtonDown;
            AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseButtonUp;
            //AssociatedObject.MouseRightButtonDown += AssociatedObject_MouseButtonDown;
            //AssociatedObject.MouseRightButtonUp += AssociatedObject_MouseButtonUp;
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseButtonDown;
            AssociatedObject.MouseLeftButtonUp -= AssociatedObject_MouseButtonUp;
            //AssociatedObject.MouseRightButtonDown -= AssociatedObject_MouseButtonDown;
            //AssociatedObject.MouseRightButtonUp -= AssociatedObject_MouseButtonUp;
        }

        private static void AssociatedObject_MouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement fe)) return;

            // コレが無いと素早い操作時に食み出て、マウスイベントを拾えなくなる(追従しない)
            fe.CaptureMouse();
        }

        private static void AssociatedObject_MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is FrameworkElement fe)) return;

            // マウスの強制補足を終了
            fe.ReleaseMouseCapture();
        }

    }
}
