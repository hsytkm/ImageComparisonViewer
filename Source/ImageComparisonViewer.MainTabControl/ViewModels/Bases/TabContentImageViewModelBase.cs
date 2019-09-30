using Control.ImagePanel.Views;
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

        private readonly IRegionManager _regionManager;
        private readonly ImageSources _imageSources;

        public DelegateCommand SwapImagesInnerCommand { get; }
        public DelegateCommand SwapImagesOuterCommand { get; }

        public TabContentImageViewModelBase(IContainerExtension container, IRegionManager regionManager, string title, int index)
            : base (title)
        {
            _regionManager = regionManager;
            _contentCount = index;
            _imageSources = container.Resolve<ImageSources>();

            SwapImagesInnerCommand = new DelegateCommand(SwapImageViewModelsInnerTrack);
            //_applicationCommands.SwapInnerTrackCommand.RegisterCommand(SwapImagesInnerCommand);

            SwapImagesOuterCommand = new DelegateCommand(SwapImageViewModelsOuterTrack);
            //_applicationCommands.SwapOuterTrackCommand.RegisterCommand(SwapImagesOuterCommand);

            IsActiveChanged += ViewModel_IsActiveChanged;
        }

        // アクティブ状態変化時の処理
        private void ViewModel_IsActiveChanged([MaybeNull]object? sender, EventArgs e)
        {
            if (!(e is DataEventArgs<bool> e2)) return;
            if (e2.Value)
            {
                // アクティブ化時
                foreach (var view in GetImageContentViews())
                {
                    // ここで各ImagePanelのソース画像を更新する
                    if (view is ImagePanel imagePanel)
                    {
                        imagePanel.UpdateImageSource();
                    }
                }
            }
            else
            {
                // 非アクティブ化時に溜まった回転情報をModelに通知する
                AdaptImageListTracks();
            }
        }

        // 画像の回転をModelに通知する
        private void AdaptImageListTracks()
        {
            _imageSources.AdaptImageListTracks(_contentCount, _innerTrackCounter);
            _innerTrackCounter = 0;
        }

        #region  GetRegionView

        /// <summary>
        /// 指定Countに対応する画像RegionのViewsを取得(2画面なら 2_0 → 2_1 を返す)
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FrameworkElement> GetImageContentViews() =>
            RegionNames.GetImageContentRegionNames(_contentCount)
                .Select(name => _regionManager.Regions[name].Views.Cast<FrameworkElement>().FirstOrDefault());

        #endregion

        #region  SwapImageViewModels

        private readonly int ImageSourcesLength = 3; //◆未確認
        private int _innerTrackCounter;

        private void IncrementInnerTrackCounter() =>
            _innerTrackCounter = (_innerTrackCounter + 1) % ImageSourcesLength;
        private void DecrementInnerTrackCounter() =>
            _innerTrackCounter = (_innerTrackCounter - 1) % ImageSourcesLength;

        /// <summary>
        /// 画像(ViewModel)を内回りで入れ替え
        /// </summary>
        private void SwapImageViewModelsInnerTrack()
        {
            if (_contentCount <= 1) return;  // 回転する必要なし
            var views = GetImageContentViews().ToList();

            var tail = views[^1].DataContext;
            for (int i = views.Count - 1; i > 0; i--)
            {
                views[i].DataContext = views[i - 1].DataContext;
            }
            views[0].DataContext = tail;

            IncrementInnerTrackCounter();
        }

        /// <summary>
        /// 画像(ViewModel)を外回りで入れ替え
        /// </summary>
        private void SwapImageViewModelsOuterTrack()
        {
            if (_contentCount <= 1) return;  // 回転する必要なし
            var views = GetImageContentViews().ToList();

            var head = views[0].DataContext;
            for (int i = 0; i < views.Count - 1; i++)
            {
                views[i].DataContext = views[i + 1].DataContext;
            }
            views[^1].DataContext = head;

            DecrementInnerTrackCounter();
        }

        #endregion

    }
}
