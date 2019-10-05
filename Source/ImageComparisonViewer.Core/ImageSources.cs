using ImageComparisonViewer.Common.Extensions;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;

namespace ImageComparisonViewer.Core
{
    public class ImageSources : BindableBase
    {
        // 画像ディレクトリの最大数
        private const int DirectriesCountMax = 3;

        //private readonly Guid guid = Guid.NewGuid();

        /// <summary>
        /// 画像元ディレクトリ(ViewModelでReplaceを監視)
        /// Scheduler指定しないとCollectionChanged中にSetして InvalidOperationException が出る
        /// </summary>
        public ReactiveCollection<string> DirectriesPath { get; } = new ReactiveCollection<string>(Scheduler.CurrentThread);
        //public ObservableCollection<string> DirectriesPath { get; } =
        //    new ObservableCollection<string>(Enumerable.Repeat<string>(default!, DirectriesCountMax));

        public ImageSources()
        {
            DirectriesPath.AddRangeOnScheduler(Enumerable.Repeat<string>(default!, DirectriesCountMax));
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

            // 'ObservableCollection during a CollectionChanged event' 回避のためコレクション操作時にSchedulerを指定する
            DirectriesPath.SetOnScheduler(index, path);
        }

        /// <summary>
        /// ドロップされたPATHをディレクトリとして設定する
        /// </summary>
        /// <param name="baseIndex"></param>
        /// <param name="droppedPaths"></param>
        public void SetDroppedPaths(int baseIndex, IReadOnlyList<string> droppedPaths)
        {
            if (baseIndex >= DirectriesCountMax)
                throw new ArgumentOutOfRangeException(nameof(baseIndex));

            var max = Math.Min(droppedPaths.Count, DirectriesCountMax);
            for (int i = 0; i < max; i++)
            {
                int index = (baseIndex + i) % DirectriesCountMax;
                var dirPath = GetDirectoryPath(droppedPaths[i]);
                SetDirectryPath(index, dirPath);
            }
        }

        /// <summary>
        /// ドロップPATHからディレクトリPATHを取得
        /// </summary>
        /// <param name="droppedPath"></param>
        /// <returns></returns>
        private static string GetDirectoryPath(string droppedPath)
        {
            if (Directory.Exists(droppedPath))
                return droppedPath;

            if (File.Exists(droppedPath))
            {
                var path = Path.GetDirectoryName(droppedPath);  // DirRootならnullになるらしい(未確認)
                return (path is null) ? droppedPath : path;
            }

            throw new FileNotFoundException(droppedPath);
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
