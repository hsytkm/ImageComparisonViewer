using Prism.Ioc;
using Prism.Modularity;
using Prism.Unity;
using System.Windows;

namespace ImageComparisonViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<Core.ImageSources>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MainTabControl.MainTabControlModule>();
            moduleCatalog.AddModule<Core.CoreModule>();
            moduleCatalog.AddModule<Control.ImagePanel.ImagePanelModule>();
        }

    }
}