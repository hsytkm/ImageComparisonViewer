using ImageComparisonViewer.Common.Prism;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using System.Windows;
using Unity;

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
            #region DI コンテナ固有の機能

            // Prism 7.x で DI コンテナ固有の機能を使いたい https://blog.okazuki.jp/entry/2019/02/05/094546
            // Unity Class LifetimeManager https://unitycontainer.github.io/api/Unity.Lifetime.LifetimeManager.html
            //IUnityContainer container = containerRegistry.GetContainer();

            // ◆Tabの切り替えでContainerにインスタンス溜まる対策 を検討したが分からなかった…
            //container.RegisterType(typeof(ImagePanel), new TransientLifetimeManager());

            #endregion

            containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
            containerRegistry.RegisterSingleton<Core.Images.CompositeImageDirectory>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MainTabControl.MainTabControlModule>();
        }

        protected override void ConfigureDefaultRegionBehaviors(IRegionBehaviorFactory regionBehaviors)
        {
            regionBehaviors.AddIfMissing(DisposeBehavior.Key, typeof(DisposeBehavior));
            base.ConfigureDefaultRegionBehaviors(regionBehaviors);
        }

    }
}