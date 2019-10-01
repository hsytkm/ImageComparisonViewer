using ImageComparisonViewer.Common.Extensions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace ImageComparisonViewer.Core
{
    public class ImageSources : BindableBase
    {
        // 画像ディレクトリの最大数
        private const int DirectriesCountMax = 3;

        // 画像元ディレクトリ(ViewModelでReplaceを監視)
        public ObservableCollection<string> DirectriesPath { get; } =
            new ObservableCollection<string>(Enumerable.Repeat<string>(default!, DirectriesCountMax));

        //private readonly Guid guid = Guid.NewGuid();

        public ImageSources()
        {

        }

        /// <summary>
        /// 画像のディレクトリPATHを設定する
        /// </summary>
        /// <param name="index"></param>
        /// <param name="path"></param>
        public void SetDirectryPath(int index, string path)
        {
            if (index >= DirectriesCountMax)
                throw new ArgumentOutOfRangeException(nameof(index));

            // NotifyCollectionChangedAction.Replace抑制のため変化時のみ設定する
            if (DirectriesPath[index] != path)
                DirectriesPath[index] = path;
        }

        /// <summary>
        /// 画像のディレクトリPATHを設定する
        /// </summary>
        /// <param name="baseIndex"></param>
        /// <param name="paths"></param>
        public void SetDirectriesPath(int baseIndex, IReadOnlyList<string> paths)
        {
            if (baseIndex >= DirectriesCountMax)
                throw new ArgumentOutOfRangeException(nameof(baseIndex));

            var max = Math.Min(paths.Count, DirectriesCountMax);
            for (int i = 0; i < max; i++)
            {
                int index = (baseIndex + i) % DirectriesCountMax;
                SetDirectryPath(index, paths[i]);
            }
        }

        public string GetDirectryPath(int index)
        {
            if (index >= DirectriesCountMax)
                throw new ArgumentOutOfRangeException(nameof(index));

            return DirectriesPath[index];
        }

        /// <summary>
        /// 外部からの回転数通知に応じてコレクション要素をシフトする
        /// </summary>
        /// <param name="contentCount">通知元の最大コンテンツ数(2画面=2)</param>
        /// <param name="rightShiftCount">右シフト回数</param>
        public void AdaptImageListTracks(int contentCount, int rightShiftCount)
        {
            if (rightShiftCount == 0) return;   // 処理不要
            if (contentCount <= 1) return;      // ループの概念ない
            var tailIndex = contentCount - 1;

            var sourceList = DirectriesPath;
            if (tailIndex > sourceList.Count - 1)
                throw new ArgumentOutOfRangeException(nameof(tailIndex));

            // 周回する分は捨てる
            rightShiftCount %= contentCount;

            if (rightShiftCount != 0)
            {
                // 対象画像数のみ回転させる
                Span<string> sourceSpan = sourceList.ToArray().AsSpan().Slice(0, tailIndex + 1);
                Span<string> sortedSpan = sourceSpan.RightShift(rightShiftCount);

                for (int i = 0; i < sortedSpan.Length; i++)
                {
                    sourceList[i] = sortedSpan[i];
                }
            }
            //Debug.WriteLine($"Model: {list[0]}, {list[1]}, {list[2]}");
        }

    }
}
