using ImageComparisonViewer.Common.Extensions;
using ImageComparisonViewer.Core.Extensions;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;

namespace ImageComparisonViewer.Core
{
    public class ImageDirectory : BindableBase
    {
        /// <summary>
        /// ディレクトリPATH
        /// </summary>
        public string DirectoryPath
        {
            get => _directoryPath;
            private set => SetProperty(ref _directoryPath, value);
        }
        private string _directoryPath = default!;

        /// <summary>
        /// 選択中のファイル(未選択ならnull)
        /// </summary>
        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            private set => SetProperty(ref _selectedFilePath, value);
        }
        private string? _selectedFilePath = default!;

        public int ContentIndex { get; }

        public ImageDirectory(int index)
        {
            ContentIndex = index;
        }

        /// <summary>
        /// 選択ディレクトリの更新
        /// </summary>
        /// <param name="sourcePath"></param>
        public void UpdateSourcePath(string sourcePath)
        {
            if (Directory.Exists(sourcePath))
            {
                // ディレクトリ内の先頭ファイルを選択
                DirectoryPath = sourcePath;
                SelectedFilePath = sourcePath.GetFirstFilePathInDirectory();
            }
            else if (File.Exists(sourcePath))
            {
                DirectoryPath = sourcePath.GetDirectoryPath();
                SetSelectedFilePath(sourcePath);
            }
            else throw new FileNotFoundException(nameof(ImageDirectory));
        }

        /// <summary>
        /// 選択ファイルの更新
        /// </summary>
        /// <param name="path"></param>
        public void SetSelectedFilePath(string? path)
        {
            SelectedFilePath = path;
        }
    }

    public class ImageSources : BindableBase
    {
        // 画像ディレクトリの最大数
        private const int DirectriesCountMax = 3;

        //private readonly Guid guid = Guid.NewGuid();

        /// <summary>
        /// 画像元ディレクトリ(ViewModelで要素のPropertyChangedを監視)
        /// Scheduler指定しないとCollectionChanged中にSetして InvalidOperationException が出る
        /// </summary>
        public ReactiveCollection<ImageDirectory> ImageDirectries { get; } = new ReactiveCollection<ImageDirectory>(Scheduler.CurrentThread);

        public ImageSources()
        {
            var items = Enumerable.Range(0, 3).Select(i => new ImageDirectory(i));
            ImageDirectries.AddRangeOnScheduler(items);
        }

        /// <summary>
        /// ドロップされたPATHを設定する
        /// </summary>
        /// <param name="index"></param>
        /// <param name="droppedPath"></param>
        public void SetDroppedPath(int index, string droppedPath)
        {
            if (index >= DirectriesCountMax)
                throw new ArgumentOutOfRangeException(nameof(index));

            ImageDirectries[index].UpdateSourcePath(droppedPath);
        }

        /// <summary>
        /// ドロップされた複数のPATHを設定する
        /// </summary>
        /// <param name="baseIndex"></param>
        /// <param name="droppedPaths"></param>
        public void SetDroppedPaths(int baseIndex, IReadOnlyList<string> droppedPaths)
        {
            if (baseIndex >= DirectriesCountMax)
                throw new ArgumentOutOfRangeException(nameof(baseIndex));

            var length = Math.Min(droppedPaths.Count, DirectriesCountMax);
            for (int i = 0; i < length; i++)
            {
                int index = (baseIndex + i) % DirectriesCountMax;
                ImageDirectries[index].UpdateSourcePath(droppedPaths[i]);
            }
        }

        /// <summary>
        /// 選択ファイルPATHの更新
        /// </summary>
        /// <param name="index"></param>
        /// <param name="droppedPath"></param>
        public void SetSelectedFlePath(int index, string? selectedFilePath)
        {
            if (index >= DirectriesCountMax)
                throw new ArgumentOutOfRangeException(nameof(index));

            ImageDirectries[index].SetSelectedFilePath(selectedFilePath);
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

            var sourceList = ImageDirectries;
            if (contentCount > sourceList.Count)
                throw new ArgumentOutOfRangeException(nameof(contentCount));

            // 周回する分は捨てる
            rightShiftCount %= contentCount;

            if (rightShiftCount != 0)
            {
                // 対象画像数のみ回転させる
                Span<ImageDirectory> sourceSpan = sourceList.ToArray().AsSpan().Slice(0, contentCount);
                Span<ImageDirectory> sortedSpan = sourceSpan.RightShift(rightShiftCount);

                for (int i = 0; i < sortedSpan.Length; i++)
                {
                    sourceList[i] = sortedSpan[i];
                }
            }
            //Debug.WriteLine($"Model: {list[0]}, {list[1]}, {list[2]}");
        }

    }
}
