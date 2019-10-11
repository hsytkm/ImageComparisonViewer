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

        public void LoadImageAsync()
        {
            Task.Run(() => Thumbnail = FilePath.ToBitmapSourceThumbnail(ThumbnailWidthMax));
            //Debug.WriteLine($"Load Thumbnail: {FilePath}");
        }

        public void UnloadImage()
        {
            Thumbnail = null;
            //Debug.WriteLine($"Unload Thumbnail: {FilePath}");
        }
    }
}
