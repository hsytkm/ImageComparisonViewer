using System;
using System.Diagnostics;
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

        /// <summary>
        /// 引数PATHを画像として読み出す
        /// </summary>
        /// <param name="imagePath">ファイルパス</param>
        /// <param name="isCanGC">メモリ不足時のGC実行可否</param>
        /// <returns></returns>
        public static BitmapImage? ToBitmapImage(this string imagePath, bool isCanGC = true)
        {
            if (!File.Exists(imagePath)) return null;   //throw new FileNotFoundException(imagePath);

            using var stream = File.Open(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var bi = new BitmapImage();
            try
            {
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = stream;
                bi.EndInit();
                if (bi.Width == 1 && bi.Height == 1) throw new OutOfMemoryException();
            }
            catch (OutOfMemoryException ex)
            {
                Debug.WriteLine($"{ex} ({Path.GetFileName(imagePath)})");

                // メモリリーク時はGCしてみる(画像表示されない現象の低減)
                // https://stackoverflow.com/questions/50040087/c-sharp-bitmapimage-width-and-height-equal-1
                if (isCanGC)
                {
                    GC.Collect();                           // アクセス不可能なオブジェクトを除去
                    GC.WaitForPendingFinalizers();          // ファイナライゼーションが終わるまでスレッド待機
                    GC.Collect();                           // ファイナライズされたばかりのオブジェクトに関連するメモリを開放
#pragma warning disable CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
                    bi = imagePath.ToBitmapImage(false);    // GC禁止で再コール
#pragma warning restore CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
                }
            }
            finally
            {
                bi?.Freeze();
            }

            return bi;
        }

    }
}
