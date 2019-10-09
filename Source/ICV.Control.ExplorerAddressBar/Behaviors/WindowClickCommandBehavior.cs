using ImageComparisonViewer.Common.Mvvm;
using Prism.Commands;
using System.Windows;

namespace ICV.Control.ExplorerAddressBar.Behaviors
{
    /// <summary>
    /// Windowクリック時にコマンド実行するビヘイビア
    /// </summary>
    class WindowClickCommandBehavior : BehaviorBase<FrameworkElement>
    {
        private static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register(
                nameof(ClickCommand),
                typeof(DelegateCommand),
                typeof(WindowClickCommandBehavior),
                new FrameworkPropertyMetadata(default!));

        public DelegateCommand ClickCommand
        {
            get => (DelegateCommand)GetValue(ClickCommandProperty);
            set => SetValue(ClickCommandProperty, value);
        }

        // ◆なんかイマイチなコード… LoadedでWindowを取得して使いまわす
        //   (UnloadedでWindow取得できなかった。原因不明)
        private Window? _window;

        protected override void OnSetup()
        {
            base.OnSetup();

            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();

            if (_window != null)
            {
                _window.MouseDown -= Window_MouseDown;
                _window = null;
            }

            AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }


        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            _window = Window.GetWindow(AssociatedObject);
            _window.MouseDown += Window_MouseDown;
        }

        private void Window_MouseDown(object sender, RoutedEventArgs e)
        {
            if (ClickCommand.CanExecute())
                ClickCommand.Execute();
        }

    }
}
