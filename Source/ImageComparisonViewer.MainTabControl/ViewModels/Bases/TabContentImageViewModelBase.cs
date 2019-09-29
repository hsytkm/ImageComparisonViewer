using ImageComparisonViewer.MainTabControl.Common;
using Prism.Commands;
using Prism.Events;
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
        public int ContentCount { get; }

        private readonly IRegionManager _regionManager;

        public DelegateCommand SwapImagesInnerCommand { get; }
        public DelegateCommand SwapImagesOuterCommand { get; }

        public TabContentImageViewModelBase(IRegionManager regionManager, string title, int index)
            : base (title)
        {
            _regionManager = regionManager;
            ContentCount = index;

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
                for (int i = 0; i < ContentCount; i++)
                {
                    foreach (var view in GetImageContentViews())
                    {
                        // ◆ここで各ImagePanelのソース画像達を指定したい
                        //if (view.DataContext is Control.ImagePanel.ViewModels.ImagePanelViewModel vmodel)
                        {
                            Debug.WriteLine("");
                            //vmodel.UpdateImageSource(i);
                        }
                    }
                }
            }
            else
            {
                // 非アクティブ化時
                // ◆ここで各ImagePanelのソース画像達を入れ替えたい
                //MainImages.AdaptImageListTracks(ContentCount);
            }
        }

        #region  GetRegionView

        /// <summary>
        /// 指定Countに対応する画像RegionのViewsを取得(2画面なら 2_0 → 2_1 を返す)
        /// </summary>
        /// <returns></returns>
        private IEnumerable<FrameworkElement> GetImageContentViews() =>
            RegionNames.GetImageContentRegionNames(ContentCount)
                .Select(name => _regionManager.Regions[name].Views.Cast<FrameworkElement>().FirstOrDefault());

        #endregion

        #region  SwapImageViewModels

        private readonly int ImageSourcesLength = 3; //◆未確認
        private int InnerTrackCounter;

        private void IncremanetInnerTrackCounter() =>
            InnerTrackCounter = (InnerTrackCounter + 1) % ImageSourcesLength;
        private void DecremanetInnerTrackCounter() =>
            InnerTrackCounter = (InnerTrackCounter - 1) % ImageSourcesLength;

        /// <summary>
        /// 画像(ViewModel)を内回りで入れ替え
        /// </summary>
        private void SwapImageViewModelsInnerTrack()
        {
            if (ContentCount <= 1) return;  // 回転する必要なし
            var views = GetImageContentViews().ToList();

            var tail = views[^1].DataContext;
            for (int i = views.Count - 1; i > 0; i--)
            {
                views[i].DataContext = views[i - 1].DataContext;
            }
            views[0].DataContext = tail;

            IncremanetInnerTrackCounter();
        }

        /// <summary>
        /// 画像(ViewModel)を外回りで入れ替え
        /// </summary>
        private void SwapImageViewModelsOuterTrack()
        {
            if (ContentCount <= 1) return;  // 回転する必要なし
            var views = GetImageContentViews().ToList();

            var head = views[0].DataContext;
            for (int i = 0; i < views.Count - 1; i++)
            {
                views[i].DataContext = views[i + 1].DataContext;
            }
            views[^1].DataContext = head;

            DecremanetInnerTrackCounter();
        }

        #endregion

    }
}
