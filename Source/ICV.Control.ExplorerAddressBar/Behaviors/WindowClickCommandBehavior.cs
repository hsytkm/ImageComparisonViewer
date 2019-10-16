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
        //   (UnloadedでWindow取得できなかった。未調査)
        private Window? _window;

        protected override void OnLoaded()
        {
            base.OnLoaded();

            _window = Window.GetWindow(AssociatedObject);
            _window.MouseDown += Window_MouseDown;
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            if (_window is null) return;
            _window.MouseDown -= Window_MouseDown;
            _window = null;
        }

        private void Window_MouseDown(object sender, RoutedEventArgs e)
        {
            if (ClickCommand.CanExecute())
                ClickCommand.Execute();
        }

    }
}
