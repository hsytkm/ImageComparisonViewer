using ImageComparisonViewer.Common.Extensions;
using ImageComparisonViewer.Core.Extensions;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ImageComparisonViewer.Core
{
    public class ImageDirectory : BindableBase
    {
        /// <summary>
        /// ディレクトリPATH(未選択ならnull)
        /// </summary>
        public string? DirectoryPath
        {
            get => _directoryPath;
            private set => SetProperty(ref _directoryPath, value);
        }
        private string? _directoryPath = default!;

        /// <summary>
        /// 選択中のファイル(未選択ならnull)
        /// </summary>
        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            private set => SetProperty(ref _selectedFilePath, value);
        }
        private string? _selectedFilePath = default!;

        public ImageDirectory() { }

        public ImageDirectory GetCopyInstance()
        {
            return new ImageDirectory()
            {
                DirectoryPath = DirectoryPath,
                SelectedFilePath = SelectedFilePath
            };
        }

        /// <summary>
        /// 選択ディレクトリの更新
        /// </summary>
        /// <param name="sourcePath"></param>
        public void UpdateBasePath(string? sourcePath)
        {
            if (sourcePath is null)
            {
                DirectoryPath = null;
                SelectedFilePath = null;
            }
            else if (Directory.Exists(sourcePath))
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

        public override string ToString() => SelectedFilePath ?? "null";
    }

    public class ImageSources : BindableBase
    {
        // 画像ディレクトリの最大数
        private const int DirectriesCountMax = 3;

        //private readonly Guid guid = Guid.NewGuid();

        /// <summary>
        /// 画像元ディレクトリ(ViewModelで各要素のPropertyChangedを監視)
        /// </summary>
        public IReadOnlyList<ImageDirectory> ImageDirectries { get; } =
            new List<ImageDirectory>(Enumerable.Range(0, DirectriesCountMax).Select(_ => new ImageDirectory()));

        public ImageSources() { }

        /// <summary>
        /// ドロップされたPATHを設定する
        /// </summary>
        /// <param name="index"></param>
        /// <param name="droppedPath"></param>
        public void SetDroppedPath(int index, string droppedPath)
        {
            if (index >= ImageDirectries.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            ImageDirectries[index].UpdateBasePath(droppedPath);
        }

        /// <summary>
        /// ドロップされた複数のPATHを設定する
        /// </summary>
        /// <param name="baseIndex"></param>
        /// <param name="droppedPaths"></param>
        public void SetDroppedPaths(int baseIndex, IReadOnlyList<string> droppedPaths)
        {
            if (baseIndex >= ImageDirectries.Count)
                throw new ArgumentOutOfRangeException(nameof(baseIndex));

            var length = Math.Min(droppedPaths.Count, ImageDirectries.Count);
            for (int i = 0; i < length; i++)
            {
                int index = (baseIndex + i) % ImageDirectries.Count;
                ImageDirectries[index].UpdateBasePath(droppedPaths[i]);
            }
        }

        /// <summary>
        /// 選択ファイルPATHの更新
        /// </summary>
        /// <param name="index"></param>
        /// <param name="selectedFilePath"></param>
        public void SetSelectedFlePath(int index, string? selectedFilePath)
        {
            if (index >= ImageDirectries.Count)
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
            var tailIndex = contentCount - 1;

            var sourceList = ImageDirectries;
            if (contentCount > sourceList.Count)
                throw new ArgumentOutOfRangeException(nameof(contentCount));

            // 周回する分は捨てる
            rightShiftCount %= contentCount;

#if false
            if (rightShiftCount != 0)
            {
                // 対象画像数のみ回転させる
                Span<ImageDirectory> sourceSpan = sourceList.ToArray().AsSpan().Slice(0, contentCount);
                Span<ImageDirectory> sortedSpan = sourceSpan.RightShift(rightShiftCount);
                for (int i = 0; i < sortedSpan.Length; i++)
                {
                    //sourceList[i] = sortedSpan[i];
                    sourceList[i].UpdateBasePath(sortedSpan[i].DirectoryPath);
                    sourceList[i].SetSelectedFilePath(sortedSpan[i].SelectedFilePath);
                }
            }
#else
            if (rightShiftCount > 0)
            {
                for (int i = 0; i < rightShiftCount; i++)
                {
                    var tail = sourceList[tailIndex].GetCopyInstance();
                    for (int j = tailIndex; j > 0; j--)
                    {
                        sourceList[j].UpdateBasePath(sourceList[j - 1].DirectoryPath);
                        sourceList[j].SetSelectedFilePath(sourceList[j - 1].SelectedFilePath);
                    }
                    sourceList[0].UpdateBasePath(tail.DirectoryPath);
                    sourceList[0].SetSelectedFilePath(tail.SelectedFilePath);
                }
            }
            else if (rightShiftCount < 0)
            {
                for (int i = 0; i < -rightShiftCount; i++)
                {
                    var head = sourceList[0].GetCopyInstance();
                    for (int j = 0; j < tailIndex; j++)
                    {
                        sourceList[j].UpdateBasePath(sourceList[j + 1].DirectoryPath);
                        sourceList[j].SetSelectedFilePath(sourceList[j + 1].SelectedFilePath);
                    }
                    sourceList[tailIndex].UpdateBasePath(head.DirectoryPath);
                    sourceList[tailIndex].SetSelectedFilePath(head.SelectedFilePath);
                }
            }
#endif
            //Debug.WriteLine($"Model: {list[0]}, {list[1]}, {list[2]}");
        }

    }
}
