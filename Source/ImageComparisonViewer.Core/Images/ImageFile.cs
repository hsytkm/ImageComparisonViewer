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
                BitmapSource? loadThumb() => FilePath.ToBitmapSourceThumbnail(ThumbnailWidthMax);
                BitmapSource rentThumb() => _imageContentBackyard.ThumbnailWarehouse.RentValue(FilePath, loadThumb);

                Thumbnail = await Task.Run(() => rentThumb());
                //Debug.WriteLine($"Load Thumbnail: {FilePath}");
            }
        }

        public void UnloadThumbnailImage()
        {
            if (Thumbnail != null)
            {
                _imageContentBackyard.ThumbnailWarehouse.ReturnValue(FilePath);
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
                BitmapSource? loadImage() => FilePath.ToBitmapImage();
                BitmapSource rentImage() => _imageContentBackyard.FullImageWarehouse.RentValue(FilePath, loadImage);

                image = await Task.Run(() => rentImage());
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
                _imageContentBackyard.FullImageWarehouse.ReturnValue(FilePath);
                FullImage = null;
                //Debug.WriteLine($"Unload FullImage: {FilePath}");
            }
        }
        #endregion

        private readonly ImageContentBackyard _imageContentBackyard;

        internal ImageFile(string path, ImageContentBackyard backyard)
        {
            FilePath = path;
            _imageContentBackyard = backyard;
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
