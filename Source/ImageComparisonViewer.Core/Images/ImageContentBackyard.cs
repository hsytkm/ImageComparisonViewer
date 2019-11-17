using ImageComparisonViewer.Common.Utils;
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace ImageComparisonViewer.Core.Images
{
    /// <summary>
    /// 読み出し画像の倉庫
    /// </summary>
    internal class ImageContentBackyard
    {
        public RefCountValueWarehouse<string, BitmapSource> ThumbnailWarehouse { get; } =
            new RefCountValueWarehouse<string, BitmapSource>();

        public RefCountValueWarehouse<string, BitmapSource> FullImageWarehouse { get; } =
            new RefCountValueWarehouse<string, BitmapSource>();
    }
}
