using ImageComparisonViewer.Common.Extensions;
using ImageComparisonViewer.Common.Utils;
using ImageComparisonViewer.Common.Wpf;
using ImageComparisonViewer.Core.Extensions;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageComparisonViewer.Core.Images
{
    /// <summary>ディレクトリ内の画像たち</summary>
    public interface IImageDirectory : INotifyPropertyChanged
    {
        /// <summary>ディレクトリPATH(未選択ならnull)</summary>
        string? DirectoryPath { get; }

        /// <summary>選択中のファイル(未選択ならnull)</summary>
        string? SelectedFilePath { get; }

        /// <summary>画像ファイルドロップ時の処理</summary>
        /// <param name="filePath"></param>
        void SetDroppedFilePath(string filePath);

        /// <summary>ディレクトリ選択時の処理</summary>
        /// <param name="dirPath"></param>
        void SetSelectedDictionaryPath(string dirPath);

        /// <summary>選択中の主画像(未選択ならnull)</summary>
        public BitmapSource? SelectedImage { get; }

        /// <summary>画像ディレクトリの読込み済みフラグ</summary>
        bool IsLoaded();

        /// <summary>画像ファイルたち</summary>
        ReadOnlyObservableCollection<ImageFile> ImageFiles { get; }

        /// <summary>サムネイルの読み出し状態を切り替える(Load+Unload)</summary>
        /// <param name="centerRatio">表示領域の中央位置の割合(0~1)</param>
        /// <param name="viewportRatio">表示領域の割合(0~1)</param>
        void UpdateThumbnails(double centerRatio, double viewportRatio);

        /// <summary>画像再読み込み(F5)</summary>
        void ReloadImageDirectory();

        /// <summary>保持リソースの破棄</summary>
        void ReleaseResources();

        /// <summary>選択画像を1つ進める</summary>
        void MoveNextImage();

        /// <summary>選択画像を1つ戻す</summary>
        void MovePrevImage();

        /// <summary>画像サチリ部の点滅</summary>
        void BlinkHighlight();
    }

    public class ImageDirectory : BindableBase, IImageDirectory, IDisposable
    {
        // 起動直後の対象ディレクトリ
        private static readonly string _defaultDirectory = //@"C:\";
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

        /// <summary>ディレクトリPATH(未選択ならnull)</summary>
        public string? DirectoryPath
        {
            get => _directoryPath;
            private set
            {
                if (SetProperty(ref _directoryPath, value))
                    UpdateDirectoryPath(_directoryPath);
            }
        }
        private string? _directoryPath = default!;

        /// <summary>ディレクトリ内の画像PATHを読み出し</summary>
        /// <param name="dirPath"></param>
        private void UpdateDirectoryPath(string? dirPath)
        {
            _imageFiles.ClearWithDispose();
            if (dirPath != null)
            {
                foreach (var path in dirPath.GetImageFilesPathInDirectory(SearchOption.TopDirectoryOnly))
                    _imageFiles.Add(new ImageFile(path, _imageContentBackyard));
            }
        }

        /// <summary>選択中のファイル(未選択ならnull)</summary>
        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            private set
            {
                if (SetProperty(ref _selectedFilePath, value))
                    _ = UpdateSelectedMainImageAsync();
            }
        }
        private string? _selectedFilePath = default!;

        /// <summary>画像ファイルドロップ時の処理</summary>
        /// <param name="filePath"></param>
        public void SetDroppedFilePath(string filePath)
        {
            var newDirPath = filePath?.ToDirectoryPath();
            if (DirectoryPath != newDirPath)
            {
                DirectoryPath = newDirPath;
            }
            else
            {
                // ディレクトリに変化がない場合は明示的に再読込みを指示する
                UpdateDirectoryPath(DirectoryPath);
            }

            // ディレクトリ更新後にファイルを選択するルール
            SelectedFilePath = filePath;

            // 同フォルダの更新だとViewportのサイズ変更が発生しないので自分でサムネイルを更新
            UpdateThumbnails(_thumbnailLoadParam);
        }

        /// <summary>ディレクトリ選択時の処理</summary>
        /// <param name="dirPath"></param>
        public void SetSelectedDictionaryPath(string dirPath)
        {
            var path = dirPath.GetFirstImageFilePathInDirectory(SearchOption.TopDirectoryOnly);
            SetDroppedFilePath(path);
            
            // ディレクトリ設定を後出しにしないと、画像が存在しないのでディレクトリがnullになってしまう…
            DirectoryPath = dirPath;
        }

        /// <summary>選択中の主画像(未選択ならnull)</summary>
        public BitmapSource? SelectedImage
        {
            get => _selectedImage;
            private set => SetProperty(ref _selectedImage, value);
        }
        private BitmapSource? _selectedImage;

        /// <summary>画像ディレクトリの読込み済みフラグ</summary>
        public bool IsLoaded() => (DirectoryPath != null && SelectedFilePath != null);

        /// <summary>画像ファイルたち</summary>
        public ReadOnlyObservableCollection<ImageFile> ImageFiles
        {
            get
            {
                if (_readOnlyImageFiles is null)
                    _readOnlyImageFiles = new ReadOnlyObservableCollection<ImageFile>(_imageFiles);
                return _readOnlyImageFiles;
            }
        }
        private ReadOnlyObservableCollection<ImageFile> _readOnlyImageFiles = default!;
        private readonly ObservableCollection<ImageFile> _imageFiles = new ObservableCollection<ImageFile>();

        /// <summary>読み出し画像の倉庫</summary>
        private readonly ImageContentBackyard _imageContentBackyard;

        internal ImageDirectory(ImageContentBackyard backyard)
        {
            _imageContentBackyard = backyard;

            SetSelectedDictionaryPath(_defaultDirectory);
        }

        // 主画像のTask管理(最終の処理のみを採用)
        private readonly CompositeCancellationTokenSource _mainImageCompositeCancellationTokenSource = new CompositeCancellationTokenSource();

        /// <summary>主画像の読み込み</summary>
        private async ValueTask UpdateSelectedMainImageAsync()
        {
            // 点滅中なら辞めさせる
            _blinkHighlightCancellationTokenSource?.Cancel();

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

        /// <summary>サムネイルの読み出し状態を切り替える(Load+Unload)</summary>
        /// <param name="centerRatio">表示領域の中央位置の割合(0~1)</param>
        /// <param name="viewportRatio">表示領域の割合(0~1)</param>
        public void UpdateThumbnails(double centerRatio, double viewportRatio)
        {
            _thumbnailLoadParam = (centerRatio, viewportRatio);
            UpdateThumbnails(_thumbnailLoadParam);
        }

        private (double centerRatio, double viewportRatio) _thumbnailLoadParam;

        private void UpdateThumbnails((double centerRatio, double viewportRatio) input)
        {
            // 画像が0個のときは0が通知される
            if (input.centerRatio == 0 || input.viewportRatio == 0) return;

            int length = ImageFiles.Count;
            if (length == 0) return;

            //Debug.WriteLine($"Thumbnail Update() center={centerRatio:f2} viewport={viewportRatio:f2}");

            int margin = 1; // 表示マージン(左右に1個余裕持たせる)
            int centerIndex = (int)Math.Floor(length * input.centerRatio);  // 切り捨て
            int countRaw = (int)Math.Ceiling(length * input.viewportRatio); // 切り上げ
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

        /// <summary>画像再読み込み(F5)</summary>
        public void ReloadImageDirectory()
        {
            var path = SelectedFilePath;

            // 更新時に選択画像が消えていた場合はディレクトリの先頭画像に切り替える
            if (!File.Exists(path))
                path = DirectoryPath?.GetFirstImageFilePathInDirectory(SearchOption.TopDirectoryOnly);

            // 同じ画像をドロップされた扱いにすることで再読み込みする
            if (path != null) SetDroppedFilePath(path);
        }

        /// <summary>保持リソースの破棄</summary>
        public void ReleaseResources() => ImageFiles.ForEach(imageFile => imageFile.ReleaseResource());

        /// <summary>選択画像を1つ進める</summary>
        public void MoveNextImage()
        {
            if (SelectedFilePath is null) return;

            // 指定条件を満たす次要素を取得
            var next = ImageFiles.NextOrTargetOrDefault(x => x.FilePath == SelectedFilePath);
            SelectedFilePath = next?.FilePath;
        }

        /// <summary>選択画像を1つ戻す</summary>
        public void MovePrevImage()
        {
            if (SelectedFilePath is null) return;

            // 指定条件を満たす前要素を取得
            var prev = ImageFiles.PrevOrTargetOrDefault(x => x.FilePath == SelectedFilePath);
            SelectedFilePath = prev?.FilePath;
        }

        #region BlinkHighlight
        private CancellationTokenSource? _blinkHighlightCancellationTokenSource;

        public void BlinkHighlight()
        {
            // 点滅中なら即終了
            if (_blinkHighlightCancellationTokenSource != null) return;

            _blinkHighlightCancellationTokenSource = new CancellationTokenSource();

            _ = BlinkHighlightAsync(_blinkHighlightCancellationTokenSource.Token)
                .ContinueWith(task =>
                {
                    _blinkHighlightCancellationTokenSource?.Dispose();
                    _blinkHighlightCancellationTokenSource = null;
                });
        }

        private async Task BlinkHighlightAsync(CancellationToken token)
        {
            var source = SelectedImage;
            if (source is null) return;

            var highlight = source.ToHighlightBitmapSource();

            // 差分(飽和画素)がなければ終わり
            if (source == highlight) return;

            int blinkCount = 4;
            int waitMsec = 500;
            for (int i = 0; i < blinkCount; i++)
            {
                if (token.IsCancellationRequested) return;

                // ◆ウェイトのためだけに画像設定Taskを作ってるのは効率悪いのでは？
                var hlTasks = new[]
                {
                    Task.Run(() => SelectedImage = highlight),
                    Task.Delay(waitMsec),
                };
                await Task.WhenAll(hlTasks);
                if (token.IsCancellationRequested) return;

                var sourceTasks = new[]
                {
                    Task.Run(() => SelectedImage = source),
                    Task.Delay(waitMsec),
                };
                await Task.WhenAll(sourceTasks);
            }
        }
        #endregion

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
                    _mainImageCompositeCancellationTokenSource.Dispose();
                    _blinkHighlightCancellationTokenSource?.Dispose();
                }

                disposedValue = true;
            }
        }

        void IDisposable.Dispose() => Dispose(true);
        #endregion
    }

}
