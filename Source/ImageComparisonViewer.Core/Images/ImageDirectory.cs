using ImageComparisonViewer.Common.Extensions;
using ImageComparisonViewer.Core.Extensions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ImageComparisonViewer.Core.Images
{
    /// <summary>
    /// 画像ディレクトリ
    /// </summary>
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
        public ObservableCollection<ImageFile> ImageFiles { get; } = new ObservableCollection<ImageFile>();

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

        /// <summary>
        /// 保持リソースの破棄
        /// </summary>
        public void ReleaseResources()
        {
            foreach (var imageFile in ImageFiles)
            {
                imageFile.UnloadImage();
            }
        }

        /// <summary>
        /// 選択画像を1つ進める
        /// </summary>
        public void MoveNextImage()
        {
            if (SelectedFilePath is null) return;

            var next = ImageFiles.NextOrTargetOrDefault(x => x.FilePath == SelectedFilePath);
            SelectedFilePath = next?.FilePath;
        }

        /// <summary>
        /// 選択画像を1つ戻す
        /// </summary>
        public void MovePrevImage()
        {
            if (SelectedFilePath is null) return;

            var prev = ImageFiles.PrevOrTargetOrDefault(x => x.FilePath == SelectedFilePath);
            SelectedFilePath = prev?.FilePath;
        }

        public override string ToString() => SelectedFilePath ?? "null";
    }
}
