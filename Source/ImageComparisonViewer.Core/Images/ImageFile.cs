using ImageComparisonViewer.Common.Wpf;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageComparisonViewer.Core.Images
{
    /// <summary>
    /// 画像ファイル
    /// </summary>
    public class ImageFile : BindableBase, IDisposable
    {
        // サムネイルの最大幅
        private const int ThumbnailWidthMax = 80;

        /// <summary>
        /// 画像ファイルのフルPATH
        /// </summary>
        public string FilePath { get; }

        #region Thumbnail
        /// <summary>
        /// サムネイル画像(非読み込み時はnull)
        /// </summary>
        public BitmapSource? Thumbnail
        {
            get => _thumbnail;
            private set => SetProperty(ref _thumbnail, value);
        }
        private BitmapSource? _thumbnail;

        public bool IsLoadThumbnailImage => Thumbnail != null;
        public bool IsUnloadThumbnailImage => Thumbnail is null;

        public async Task LoadThumbnailImageAsync()
        {
            if (Thumbnail is null)
            {
                Thumbnail = await Task.Run(() => FilePath.ToBitmapSourceThumbnail(ThumbnailWidthMax));
                //Debug.WriteLine($"Load Thumbnail: {FilePath}");
            }
        }

        public void UnloadThumbnailImage()
        {
            if (Thumbnail != null)
            {
                Thumbnail = null;
                //Debug.WriteLine($"Unload Thumbnail: {FilePath}");
            }
        }
        #endregion

        #region FullImage
        /// <summary>
        /// 主画像(非読み込み時はnull)
        /// </summary>
        public BitmapSource? FullImage
        {
            get => _fullImage;
            private set => SetProperty(ref _fullImage, value);
        }
        private BitmapSource? _fullImage;

        public async Task<BitmapSource?> LoadFullImageAsync(CancellationToken cancelToken)
        {
            var image = FullImage;
            if (image is null)
            {
                image = await Task.Run(() => FilePath.ToBitmapImage());
                //Debug.WriteLine($"Load FullImage: {FilePath}");

                if (cancelToken.IsCancellationRequested)
                {
                    //Debug.WriteLine($"Discard FullImage: {FilePath}");
                    return null;
                }
                FullImage = image;
            }
            return image;
        }

        public void UnloadFullImage()
        {
            if (FullImage != null)
            {
                FullImage = null;
                //Debug.WriteLine($"Unload FullImage: {FilePath}");
            }
        }
        #endregion

        public ImageFile(string path)
        {
            FilePath = path;
        }

        public void ReleaseResource()
        {
            UnloadThumbnailImage();
            UnloadFullImage();
        }

        public void Dispose()
        {
            //Debug.WriteLine($"Dispose: {FilePath}");
            ReleaseResource();
        }

    }
}
