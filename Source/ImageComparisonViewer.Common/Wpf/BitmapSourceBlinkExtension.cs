using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace ImageComparisonViewer.Common.Wpf
{
    public static class BitmapSourceBlinkExtension
    {
        /// <summary>
        /// 引数画像の飽和画素を塗り潰した画像を返す
        /// </summary>
        /// <param name="source">塗り潰しなしの通常画像</param>
        /// <returns>塗り潰し画像</returns>
        public static BitmapSource ToHighlightBitmapSource(this BitmapSource source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            return source.ToHighlightBitmapSourceSub();

            //var sw = new Stopwatch();
            //sw.Restart();
            //var image = source.ToHighlightBitmapSource2();
            //sw.Stop();

            //Debug.WriteLine($"{sw.ElapsedMilliseconds}ms");
            //return image;
        }

        private static BitmapSource ToHighlightBitmapSourceSub(this BitmapSource source)
        {
            int height = source.PixelHeight;
            int width = source.PixelWidth;
            int bytesPerPixel = (source.Format.BitsPerPixel + 7) / 8;
            int stride = width * bytesPerPixel;

            // 1Pixel=4バイト以外は未実装
            if (bytesPerPixel != 4) throw new NotImplementedException($"BytesPerPixel={bytesPerPixel}");

            var dstData = new byte[height * stride];
            source.CopyPixels(dstData, stride, 0);

            bool rewrite = false;
            for (int i = 0; i < dstData.Length; i += bytesPerPixel)
            {
                // 4バイトなら AARRGGBB の並び
                ref var pixel = ref Unsafe.As<byte, UInt32>(ref dstData[i]);

                // Bサチリチェック
                if ((pixel & 0x0000_00ff) == 0x0000_00ff)
                {
                    // 3色:RGBサチリならRGBを0
                    if ((pixel & 0x00ff_ffff) == 0x00ff_ffff)
                    {
                        pixel &= 0xff00_0000;
                    }
                    // 2色:-GBのみサチリならR--を0
                    else if ((pixel & 0x0000_ffff) == 0x0000_ffff)
                    {
                        pixel &= 0xff00_ffff;
                    }
                    // 2色:R-Bのみサチリなら-G-を0
                    else if ((pixel & 0x00ff_00ff) == 0x00ff_00ff)
                    {
                        pixel &= 0xffff_00ff;
                    }
                    // 1色:--BのみサチリならRG-を0
                    else
                    {
                        pixel &= 0xff00_00ff;
                    }
                    rewrite = true;
                }
                // Gサチリチェック
                else if ((pixel & 0x0000_ff00) == 0x0000_ff00)
                {
                    // 2色:RG-のみサチリなら--Bを0
                    if ((pixel & 0x00ff_ff00) == 0x00ff_ff00)
                    {
                        pixel &= 0xffff_ff00;
                    }
                    // 1色:-G-のみサチリならR-Bを0
                    else
                    {
                        pixel &= 0xff00_ff00;
                    }
                    rewrite = true;
                }
                // Rサチリチェック
                else if ((pixel & 0x00ff_0000) == 0x00ff_0000)
                {
                    // 1色:R--のみサチリなら-GBを0
                    pixel &= 0xffff_0000;
                    rewrite = true;
                }
            }

            // 書き換えなければ元情報をそのまま返す
            if (!rewrite) return source;

            var bs = BitmapSource.Create(width, height,
                source.DpiX, source.DpiY, source.Format,
                null, dstData, stride);
            bs.Freeze();

            return bs;
        }

    }
}
