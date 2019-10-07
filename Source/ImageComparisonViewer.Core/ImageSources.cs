using ImageComparisonViewer.Common.Extensions;
using ImageComparisonViewer.Common.Wpf;
using ImageComparisonViewer.Core.Extensions;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ImageComparisonViewer.Core
{
    public class ImageFile : BindableBase
    {
        // サムネイルの最大幅
        private const int ThumbnailWidthMax = 80;

        /// <summary>
        /// 画像ファイルのフルPATH
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// サムネイル画像(非読み込み時はnull)
        /// </summary>
        public BitmapSource? Thumbnail
        {
            get => _thumbnail;
            private set => SetProperty(ref _thumbnail, value);
        }
        private BitmapSource? _thumbnail;

        public bool IsLoadImage => !(Thumbnail is null);
        public bool IsUnloadImage => Thumbnail is null;

        public ImageFile(string path)
        {
            FilePath = path;
        }

        public void LoadImage()
        {
            Thumbnail = FilePath.ToBitmapSourceThumbnail(ThumbnailWidthMax);
            //Debug.WriteLine($"Finish: {FilePath}");
        }

        public void UnloadImage() => Thumbnail = null;
    }

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

        /// <summary>
        /// 画像ファイルたち
        /// </summary>
        public ObservableCollection<ImageFile> ImageFiles
        {
            get => _imageFiles;
            private set => SetProperty(ref _imageFiles, value);
        }
        private ObservableCollection<ImageFile> _imageFiles = new ObservableCollection<ImageFile>();

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
            else
            {
                throw new FileNotFoundException(nameof(ImageDirectory));
            }

            // ディレクトリ内の画像PATHを読み出し
            ImageFiles.Clear();
            if (DirectoryPath != null)
            {
                foreach (var imageFile in GetImageFiles(DirectoryPath))
                    ImageFiles.Add(imageFile);
            }
        }

        /// <summary>
        /// 選択ファイルの更新
        /// </summary>
        /// <param name="path"></param>
        public void SetSelectedFilePath(string? path)
        {
            SelectedFilePath = path;
        }

        private static IEnumerable<ImageFile> GetImageFiles(string directoryPath)
        {
            if (directoryPath is null) yield break; //=Enumerable.Empty<ImageFile>();

            foreach (var path in Directory.EnumerateFiles(directoryPath, "*.jpg", SearchOption.TopDirectoryOnly))
            {
                yield return new ImageFile(path);
            }
        }

        /// <summary>
        /// サムネイルの読み出し状態を切り替える(Load+Unload)
        /// </summary>
        /// <param name="centerRatio">表示領域の中央位置の割合(0~1)</param>
        /// <param name="viewportRatio">表示領域の割合(0~1)</param>
        public void UpdateThumbnail(double centerRatio, double viewportRatio)
        {
            // 画像が0個のときは0が通知される
            if (centerRatio == 0 || viewportRatio == 0) return;

            var list = ImageFiles;
            int length = list.Count;
            if (length == 0) return;

            //Debug.WriteLine($"Thumbnail Update() center={centerRatio:f2} viewport={viewportRatio:f2}");

            int margin = 1; // 表示マージン(左右に1個余裕持たせる)
            int centerIndex = (int)Math.Floor(length * centerRatio);        // 切り捨て
            int countRaw = (int)Math.Ceiling(length * viewportRatio);       // 切り上げ
            int start = Math.Max(0, centerIndex - (countRaw / 2) - margin); // 一つ余分に描画する
            int end = Math.Min(length - 1, start + countRaw + margin);      // 一つ余分に描画する
            int count = end - start + 1;
            //Debug.WriteLine($"Thumbnail Update() total={length} start={start} end={end} count={count}");

            // 解放リスト(表示範囲外で読込み中)
            var unloadThumbs = Enumerable.Range(0, length)
                .Where(x => !(start <= x && x <= end))
                .Select(x => list[x])
                .Where(x => x.IsLoadImage);
            foreach (var thumb in unloadThumbs)
            {
                //Debug.WriteLine($"Thumbnail Update() Unload: {thumb.FilePath}");
                thumb.UnloadImage();
            }

            // 読込みリスト(表示範囲の未読込みを対象)
            var loadThumbs = Enumerable.Range(start, count)
                .Select(x => list[x])
                .Where(x => x.IsUnloadImage);
            foreach (var thumb in loadThumbs)
            {
                //Debug.WriteLine($"Thumbnail Update() Load: {thumb.FilePath}");
                //Task.Run(() => thumb.LoadImage()); // 完了を待たない(高速化)
                thumb.LoadImage();
            }

            // 読み込み状況の表示テスト
            LoadedItemText();
        }

        /// <summary>
        /// 読み込み状況の表示テスト
        /// ◆アイテムが全て画面内に収まっているとScrollChangedが発生せず更新されないが、デバッグ用やからいいや
        /// </summary>

        [Conditional("DEBUG")]
        private void LoadedItemText()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var thumb in ImageFiles)
            {
                sb.Append(thumb.IsUnloadImage ? "□" : "■");
            }
            Debug.WriteLine(sb.ToString());
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
