using ImageComparisonViewer.Common.Prism;
using ImageComparisonViewer.Core.Extensions;
using ImageComparisonViewer.Core.Images;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Unity;

namespace ImageComparisonViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        public App()
        {
            Startup += Startup_CommandLineArgs;
        }

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
            containerRegistry.RegisterSingleton<ICompositeImageDirectory, CompositeImageDirectory>();

            LoadStartupImages(Container);
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

        #region CommandLineArgs
        private static IReadOnlyList<string>? _commandLineImagePathArgs = default;

        private void Startup_CommandLineArgs(object sender, StartupEventArgs e)
        {
            // 有効な画像PATHのみをコマンドライン引数として採用
            if (e.Args.Length > 0)
                _commandLineImagePathArgs = e.Args.Where(arg => arg.IsSupportedImagePath()).ToList();
        }

        private static void LoadStartupImages(IContainerProvider container)
        {
            var paths = _commandLineImagePathArgs;
            if (paths?.Count > 0)
            {
                var compositeDirectory = container.Resolve<ICompositeImageDirectory>();
                var tabImageUpdate = compositeDirectory.SetDroppedPaths(baseIndex: 0, paths);

                var applicationCommands = container.Resolve<IApplicationCommands>();
                applicationCommands.OnInitializedTabContentImageCount = tabImageUpdate;
            }
            _commandLineImagePathArgs = null;
        }
        #endregion

    }
}