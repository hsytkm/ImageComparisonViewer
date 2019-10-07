﻿using ImageComparisonViewer.ImagePanels.Views;
using ImageComparisonViewer.Common.Extensions;
using ImageComparisonViewer.Core;
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
using System.Windows;

namespace ImageComparisonViewer.MainTabControl.ViewModels.Bases
{
    abstract class TabContentImageViewModelBase : TabContentViewModelBase
    {
        private readonly int _contentCount;

        private readonly IContainerExtension _container;
        private readonly IRegionManager _regionManager;
        private readonly ImageSources _imageSources;

        public DelegateCommand ImagesRightShiftCommand { get; }
        public DelegateCommand ImagesLeftShiftCommand { get; }

        public TabContentImageViewModelBase(IContainerExtension container, IRegionManager regionManager, string title, int index)
            : base(title)
        {
            _container = container;
            _regionManager = regionManager;
            _contentCount = index;
            _imageSources = container.Resolve<ImageSources>();

            ImagesRightShiftCommand = new DelegateCommand(() =>
            {
                RightShiftViewModels();
                IncrementRightShiftCounter();   // ユーザ操作による回転数を更新
            });
            //_applicationCommands.SwapInnerTrackCommand.RegisterCommand(RightShiftCommand);

            ImagesLeftShiftCommand = new DelegateCommand(() =>
            {
                LeftShiftViewModels();
                IncrementLeftShiftCounter();   // ユーザ操作による回転数を更新
            });
            //_applicationCommands.SwapOuterTrackCommand.RegisterCommand(LeftShiftCommand);

            IsActiveChanged += ViewModel_IsActiveChanged;
        }

        // アクティブ状態変化時の処理
        private void ViewModel_IsActiveChanged([MaybeNull]object? sender, EventArgs e)
        {
            if (!(e is DataEventArgs<bool> e2)) return;
            var isActive = e2.Value;

            // 非アクティブ時に溜まった回転数をModelに通知する
            if (isActive)
            {
                var regionNames = RegionNames.GetImageContentRegionNames(_contentCount);
                foreach (var (name, index) in regionNames.Indexed())
                {
                    //_regionManager.RegisterViewWithRegion(name, () =>
                    //{
                    //    // ◆複数の引数を渡す場合はデータstructに変えましょう
                    //    //var parameters = new[] {
                    //    //    (typeof(int), (object)index),
                    //    //    (typeof(uint), (object)((uint)_contentCount)),
                    //    //};
                    //    //return _container.Resolve<ImagePanel>(parameters);

                    //    // ◆上のコードだとTabの切り替えでContainerにゴミインスタンスが溜まって
                    //    //   メモリリークするので普通にインスタンス作る
                    //    return new ImagePanel(_container, _regionManager, index, (uint)_contentCount);
                    //});

                    var view = new ImagePanel(_container, _regionManager, index, (uint)_contentCount);
                    _regionManager.AddToRegion(name, view);
                }
            }
            else
            {
                AdaptImagesShift();

                foreach (var name in RegionNames.GetImageContentRegionNames(_contentCount))
                {
                    _regionManager.Regions[name].RemoveAll();
                }
            }
        }

        #region  GetRegionView

        /// <summary>
        /// 指定Countに対応する画像RegionのViewsを取得(2画面なら 2_0 → 2_1 を返す)
        /// </summary>
        /// <returns></returns>
        private IEnumerable<T> GetImageContentViews<T>() where T : FrameworkElement =>
            RegionNames.GetImageContentRegionNames(_contentCount)
                .Select(name => _regionManager.Regions[name].Views.Cast<T>().FirstOrDefault());

        #endregion

        #region  ShiftImageViewModels

        private int _rightShiftCounter;

        private void IncrementRightShiftCounter() => _rightShiftCounter++;
        private void IncrementLeftShiftCounter() => _rightShiftCounter--;

        /// <summary>
        /// 画像の回転を外部に通知する(+次に備えてViewModelを元に戻す)
        /// </summary>
        private void AdaptImagesShift()
        {
            // ユーザの指示で回転させたViewModelを元に戻す(◆ややこしい…)
            RightShiftViewModels(-_rightShiftCounter);

            // Modelに溜まった回転数を通知
            _imageSources.AdaptImageListTracks(_contentCount, _rightShiftCounter);

            // 外部通知したらクリアする
            _rightShiftCounter = 0;
        }

        /// <summary>
        /// 画像(ViewModel)を右回りで入れ替え
        /// </summary>
        /// <param name="rightShift">右シフト回数</param>
        private void RightShiftViewModels(int rightShift = 1)
        {
            if (_contentCount <= 1) return;  // 回転する必要なし
            if (rightShift == 0) return;

#if true
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
