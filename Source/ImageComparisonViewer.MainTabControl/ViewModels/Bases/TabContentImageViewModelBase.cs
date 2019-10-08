using ImageComparisonViewer.ImagePanels.Views;
using ImageComparisonViewer.Common.Extensions;
using ImageComparisonViewer.Core.Images;
using ImageComparisonViewer.MainTabControl.Common;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ImageComparisonViewer.Common.Mvvm;

namespace ImageComparisonViewer.MainTabControl.ViewModels.Bases
{
    abstract class TabContentImageViewModelBase : TabContentViewModelBase
    {
        private readonly int _contentCount;

        private readonly IContainerExtension _container;
        private readonly IRegionManager _regionManager;
        private readonly CompositeImageDirectory _compositeDirectory;

        public DelegateCommand ImagesRightShiftCommand { get; }
        public DelegateCommand ImagesLeftShiftCommand { get; }

        public TabContentImageViewModelBase(IContainerExtension container, IRegionManager regionManager, string title, int index)
            : base(title)
        {
            _container = container;
            _regionManager = regionManager;
            _contentCount = index;
            _compositeDirectory = container.Resolve<CompositeImageDirectory>();

            ImagesRightShiftCommand = new DelegateCommand(() => RightShiftViewModels());
            //_applicationCommands.SwapInnerTrackCommand.RegisterCommand(RightShiftCommand);

            ImagesLeftShiftCommand = new DelegateCommand(() => LeftShiftViewModels());
            //_applicationCommands.SwapOuterTrackCommand.RegisterCommand(LeftShiftCommand);

            IsActiveChanged += ViewModel_IsActiveChanged;
        }

        // アクティブ状態変化時の処理
        private void ViewModel_IsActiveChanged([MaybeNull]object? sender, EventArgs e)
        {
            if (!(e is DataEventArgs<bool> e2)) return;
            var isActive = e2.Value;

            if (isActive)
            {
                RegisterImagePanelViewRegions();

                // Modelへのリソース破棄要求(1画面に切り替わったら2画面以上の情報は捨てる)
                _compositeDirectory.ReleaseResources(_contentCount);
            }
            else
            {
                RemoveImagePanelViewRegions();

                // 非アクティブ時に溜まった回転数をModelに通知する
                AdaptImagesShift();
            }
        }

        #region  ViewRegion

        /// <summary>
        /// 指定Countに対応する画像RegionのViewsを取得(2画面なら 2_0 → 2_1 を返す)
        /// </summary>
        /// <returns></returns>
        //private IEnumerable<T> GetImageContentViews<T>() where T : FrameworkElement =>
        //    RegionNames.GetImageContentRegionNames(_contentCount)
        //        .Select(name => _regionManager.Regions[name].Views.Cast<T>().FirstOrDefault());

        /// <summary>
        /// Viewを作成してリージョンに登録する
        /// </summary>
        private void RegisterImagePanelViewRegions(int contentIndexOffset = 0)
        {
            var regionNames = RegionNames.GetImageContentRegionNames(_contentCount);
            foreach (var (name, index) in regionNames.Indexed())
            {
                _regionManager.RegisterViewWithRegion(name, () =>
                {
                    var contentIndex = (index + contentIndexOffset) % _contentCount;
                    var parameters = ImageViewParameterFactory.GetImageViewParameters(contentIndex, _contentCount);
                    return _container.Resolve<ImagePanel>(parameters);
                });
            }
        }

        /// <summary>
        /// リージョンのViewを削除する
        /// </summary>
        private void RemoveImagePanelViewRegions()
        {
            foreach (var name in RegionNames.GetImageContentRegionNames(_contentCount))
            {
                _regionManager.Regions[name].RemoveAll();
            }
        }

        #endregion

        #region  ShiftImageViewModels

        // ViewModelで管理している右回転数(左回転時は負数になる)
        private int _rightShiftCounter;

        /// <summary>
        /// 画像の回転を外部に通知する(+次に備えてViewModelを元に戻す)
        /// </summary>
        private void AdaptImagesShift()
        {
            // 溜まった回転数をModelに通知
            _compositeDirectory.AdaptImageListTracks(_contentCount, _rightShiftCounter);

            // 外部通知したらクリアする
            _rightShiftCounter = 0;
        }

        /// <summary>
        /// 画像(ViewModel)を右回りで入れ替え
        /// </summary>
        /// <param name="rightShiftRequest">右シフト回数</param>
        private void RightShiftViewModels(int rightShiftRequest = 1)
        {
            if (_contentCount <= 1) return;  // 回転する必要なし
            if (rightShiftRequest == 0) return;

#if true
            // Vの作り直し
            RemoveImagePanelViewRegions();

            // 画像コンテンツ番号のシフト量(これまでの累積回転数を考慮)
            var indexShift = (-(_rightShiftCounter + rightShiftRequest)) % _contentCount;
            if (indexShift < 0) indexShift += _contentCount;
            RegisterImagePanelViewRegions(indexShift);

#elif false
            // Vの入れ替え
            var views = GetImageContentViews<FrameworkElement>().ToList().RightShift(rightShift);
            foreach (var (name, index) in RegionNames.GetImageContentRegionNames(_contentCount).Indexed())
            {
                _regionManager.Regions[name].RemoveAll();
                _regionManager.RegisterViewWithRegion(name, () => views[index]);
            }
#else
            // VMの入れ替え
            var views = GetImageContentViews<FrameworkElement>().ToArray();
            var vmodels = views.Select(x => x.DataContext).ToArray().AsSpan()
                .RightShift(rightShift);

            for (int i = 0; i < views.Length; i++)
            {
                views[i].DataContext = vmodels[i];
            }
#endif
            // ViewModel内の回転数を加算
            _rightShiftCounter += rightShiftRequest;
        }

        /// <summary>
        /// 画像(ViewModel)を左回りで入れ替え
        /// </summary>
        /// <param name="leftShift">左シフト回数</param>
        private void LeftShiftViewModels(int leftShift = 1) =>
            RightShiftViewModels(-leftShift);   // 右シフトの逆

        #endregion

    }
}
