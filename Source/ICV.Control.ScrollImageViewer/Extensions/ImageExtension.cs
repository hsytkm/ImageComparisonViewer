using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ICV.Control.ScrollImageViewer.Extensions
{
    static class ImageExtension
    {
        /// <summary>Image内の現画像のPixelサイズを取得する</summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static Size GetImageSourcePixelSize(this Image image)
        {
            if (image?.Source is BitmapSource source)
                return new Size(source.PixelWidth, source.PixelHeight);
            return default;
        }

    }
}
