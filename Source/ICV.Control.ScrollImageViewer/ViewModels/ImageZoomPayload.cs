using System;

namespace ICV.Control.ScrollImageViewer.ViewModels
{
    public readonly struct ImageZoomPayload
    {
        /// <summary>全体表示フラグ</summary>
        public readonly bool IsEntire;

        /// <summary>ズーム倍率(等倍=1.0)</summary>
        public readonly double MagRatio;

        public ImageZoomPayload(bool entire, double mag)
        {
            (IsEntire, MagRatio) = (entire, mag);
        }
    }
}
