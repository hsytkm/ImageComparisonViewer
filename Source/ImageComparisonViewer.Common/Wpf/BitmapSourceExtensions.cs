using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageComparisonViewer.Common.Wpf
{
    public static class BitmapSourceExtensions
    {
        /// <summary>
        /// 引数PATHをサムネイルとして読み出す
        /// </summary>
        /// <param name="imagePath">ファイルパス</param>
        /// <param name="widthMax">サムネイルの画像幅</param>
        /// <returns></returns>
        public static BitmapSource? ToBitmapSourceThumbnail(this string imagePath, double widthMax)
        {
            if (!File.Exists(imagePath)) return null;   //throw new FileNotFoundException(imagePath);

            using var stream = File.Open(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var img = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
            var bitmapSource = img.Thumbnail ?? (img as BitmapSource);

            var longSide = Math.Max(bitmapSource.PixelWidth, bitmapSource.PixelHeight);
            var scale = widthMax / (double)longSide;
            var thumbnail = new TransformedBitmap(bitmapSource, new ScaleTransform(scale, scale));
            var cachedBitmap = new CachedBitmap(thumbnail, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            cachedBitmap.Freeze();

            return (BitmapSource)cachedBitmap;  // アップキャストは常に合法
        }

    }
}
