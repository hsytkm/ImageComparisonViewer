using ImageComparisonViewer.Common.Extensions;
using ImageComparisonViewer.Common.Utils;
using ImageComparisonViewer.Core.Extensions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageComparisonViewer.Core.Images
{
    /// <summary>
    /// 画像ディレクトリ
    /// </summary>
    public class ImageDirectory : BindableBase, IDisposable
    {
        /// <summary>
        /// ディレクトリPATH(未選択ならnull)
        /// </summary>
        public string? DirectoryPath
        {
            get => _directoryPath;
            set
            {
                if (SetProperty(ref _directoryPath, value))
                {
                    // ディレクトリ内の画像PATHを読み出し
                    _imageFiles.ClearWithDispose();
                    if (value != null)
                    {
                        foreach (var path in value.GetImageFilesPathInDirectory())
                            _imageFiles.Add(new ImageFile(path));
                    }

                    // ディレクトリが変化したら先頭ファイルに上書きする
                    if (SelectedFilePath?.ToDirectoryPath() != value)
                    {
                        SelectedFilePath = value?.GetFirstImageFilePathInDirectory(SearchOption.TopDirectoryOnly);
                    }
                }
            }
        }
        private string? _directoryPath = default!;

        /// <summary>
        /// 選択中のファイル(未選択ならnull)
        /// </summary>
        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                if (SetProperty(ref _selectedFilePath, value))
                {
                    // ファイルドロップ時のディレクトリ変更(常に書き込みしてOK)
                    DirectoryPath = value?.ToDirectoryPath();

                    // ディレクトリ更新後に主画像を読み込み
                    _ = UpdateSelectedMainImageAsync();
                }
            }
        }
        private string? _selectedFilePath = default!;

        /// <summary>
        /// 選択中の主画像(未選択ならnull)
        /// </summary>
        public BitmapSource? SelectedImage
        {
            get => _selectedImage;
            private set => SetProperty(ref _selectedImage, value);
        }
        private BitmapSource? _selectedImage;

        /// <summary>
        /// 画像ファイルたち
        /// </summary>
        public ReadOnlyObservableCollection<ImageFile> ImageFiles => new ReadOnlyObservableCollection<ImageFile>(_imageFiles);
        private readonly ObservableCollection<ImageFile> _imageFiles = new ObservableCollection<ImageFile>();

        public ImageDirectory() { }

        // 主画像のTask管理(最終の処理のみを採用)
        private readonly CompositeCancellationTokenSource _mainImageCompositeCancellationTokenSource = new CompositeCancellationTokenSource();

        /// <summary>
        /// 主画像の読み込み
        /// </summary>
        private async Task UpdateSelectedMainImageAsync()
        {
            var imagePath = SelectedFilePath;
            if (imagePath is null)
            {
                SelectedImage = null;
                return;
            }

            // 以下なら更新不要(いる?)
            //if (SelectedImage?.StreamSource is FileStream fs && fs.Name == imagePath) return;

            var load = ImageFiles.FirstOrDefault(x => x.FilePath == imagePath);
            if (load != null)
            {
                var cancelToken = _mainImageCompositeCancellationTokenSource.GetCancellationToken();
                var image = await load.LoadFullImageAsync(cancelToken);

                if (cancelToken.IsCancellationRequested)
                {
                    //Debug.WriteLine($"Discard FullImage: {load.FilePath}");
                    return;
                }

                // 正常終了時に処理中のTokenをクリア
                _mainImageCompositeCancellationTokenSource.Clear();
                SelectedImage = image;
            }

            // 画像読出し完了後に他画像を開放
            ImageFiles.Where(x => x.FilePath != imagePath).ForEach(unload => unload.UnloadFullImage());
        }

        /// <summary>
        /// サムネイルの読み出し状態を切り替える(Load+Unload)
        /// </summary>
        /// <param name="centerRatio">表示領域の中央位置の割合(0~1)</param>
        /// <param name="viewportRatio">表示領域の割合(0~1)</param>
        public void UpdateThumbnails(double centerRatio, double viewportRatio)
        {
            // 画像が0個のときは0が通知される
            if (centerRatio == 0 || viewportRatio == 0) return;

            int length = ImageFiles.Count;
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
                .Select(x => ImageFiles[x]);
            foreach (var thumb in unloadThumbs)
            {
                thumb.UnloadThumbnailImage();
            }

            // 読込みリスト(表示範囲の未読込みを対象)
            var loadThumbs = Enumerable.Range(start, count)
                .Select(x => ImageFiles[x]);
            foreach (var thumb in loadThumbs)
            {
                // Asyncの完了を待たない(高速化)
                _ = thumb.LoadThumbnailImageAsync();
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
                sb.Append(thumb.IsUnloadThumbnailImage ? "□" : "■");
            }
            Debug.WriteLine(sb.ToString());
        }

        /// <summary>
        /// 保持リソースの破棄
        /// </summary>
        public void ReleaseResources() => ImageFiles.ForEach(imageFile => imageFile.ReleaseResource());

        /// <summary>
        /// 選択画像を1つ進める
        /// </summary>
        public void MoveNextImage()
        {
            if (SelectedFilePath is null) return;

            // 指定条件を満たす次要素を取得
            var next = ImageFiles.NextOrTargetOrDefault(x => x.FilePath == SelectedFilePath);
            SelectedFilePath = next?.FilePath;
        }

        /// <summary>
        /// 選択画像を1つ戻す
        /// </summary>
        public void MovePrevImage()
        {
            if (SelectedFilePath is null) return;

            // 指定条件を満たす前要素を取得
            var prev = ImageFiles.PrevOrTargetOrDefault(x => x.FilePath == SelectedFilePath);
            SelectedFilePath = prev?.FilePath;
        }

        public override string ToString() => SelectedFilePath ?? "null";

        #region IDisposable Support
        private bool disposedValue = false; // 重複する呼び出しを検出するには

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: マネージ状態を破棄します (マネージ オブジェクト)。
                }
                _mainImageCompositeCancellationTokenSource.Dispose();

                disposedValue = true;
            }
        }

        void IDisposable.Dispose() => Dispose(true);
        #endregion
    }

}
