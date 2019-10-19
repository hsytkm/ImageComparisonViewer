using ImageComparisonViewer.Common.Wpf;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageComparisonViewer.Core.Images
{
    /// <summary>
    /// 画像ファイル
    /// </summary>
    public class ImageFile : BindableBase
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

        public bool IsLoadThumbnailImage => !(Thumbnail is null);
        public bool IsUnloadThumbnailImage => Thumbnail is null;

        public void LoadThumbnailImageAsync()
        {
            if (Thumbnail is null)
            {
                Task.Run(() => Thumbnail = FilePath.ToBitmapSourceThumbnail(ThumbnailWidthMax));
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

        public async Task LoadFullImageAsync()
        {
            if (FullImage is null)
            {
                await Task.Run(() => FullImage = FilePath.ToBitmapImage());
                //Debug.WriteLine($"Load FullImage: {FilePath}");
            }
        }

        public void LoadFullImage()
        {
            if (FullImage is null)
            {
                FullImage = FilePath.ToBitmapImage();
                //Debug.WriteLine($"Load FullImage: {FilePath}");
            }
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

    }
}
