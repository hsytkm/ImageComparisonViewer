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

        public async ValueTask LoadThumbnailImageAsync()
        {
            if (Thumbnail is null)
            {
                var warehouse = _imageContentBackyard.ThumbnailWarehouse;

                var image = warehouse.RentValueIfExist(FilePath);
                if (image is null)
                {
                    // 辞書に存在しないのでTaskで登録
                    BitmapSource? loadThumb() => FilePath.ToBitmapSourceThumbnail(ThumbnailWidthMax);
                    image = await Task.Run(() => warehouse.RentValue(FilePath, loadThumb));
                }
                //Debug.WriteLine($"Load Thumbnail: {FilePath}");
                Thumbnail = image;
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

        public async ValueTask<BitmapSource?> LoadFullImageAsync(CancellationToken cancelToken)
        {
            if (FullImage is null)
            {
                var warehouse = _imageContentBackyard.FullImageWarehouse;

                var image = warehouse.RentValueIfExist(FilePath);
                if (image is null)
                {
                    BitmapSource? loadImage() => FilePath.ToBitmapImage();
                    image = await Task.Run(() => warehouse.RentValue(FilePath, loadImage));
                }
                //Debug.WriteLine($"Load FullImage: {FilePath}");

                if (cancelToken.IsCancellationRequested)
                {
                    //Debug.WriteLine($"Discard FullImage: {FilePath}");
                    return null;
                }
                FullImage = image;
            }
            return FullImage;
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
