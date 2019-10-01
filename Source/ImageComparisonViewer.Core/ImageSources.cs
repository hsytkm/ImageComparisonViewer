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

            var list = DirectriesPath;
            if (tailIndex > list.Count - 1)
                throw new ArgumentOutOfRangeException(nameof(tailIndex));

            // 周回する分は捨てる
            rightShiftCount %= contentCount;

            Debug.WriteLine("◆拡張メソッドに置換したい");
            if (rightShiftCount > 0)
            {
                for (int i = 0; i < rightShiftCount; i++)
                {
                    var tail = list[tailIndex];
                    for (int j = tailIndex; j > 0; j--)
                    {
                        list[j] = list[j - 1];
                    }
                    list[0] = tail;
                }
            }
            else if (rightShiftCount < 0)
            {
                for (int i = 0; i < -rightShiftCount; i++)
                {
                    var head = list[0];
                    for (int j = 0; j < tailIndex; j++)
                    {
                        list[j] = list[j + 1];
                    }
                    list[tailIndex] = head;
                }
            }
            //Debug.WriteLine($"Model: {list[0]}, {list[1]}, {list[2]}");
        }

    }
}
