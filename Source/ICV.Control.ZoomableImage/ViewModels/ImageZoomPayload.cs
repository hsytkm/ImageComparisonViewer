using System;

namespace ICV.Control.ZoomableImage.ViewModels
{
    public readonly struct ImageZoomPayload
    {
        public readonly bool IsEntire;
        public readonly double MagRatio;

        public ImageZoomPayload(bool entire, double mag)
        {
            IsEntire = entire;
            MagRatio = mag;
        }

    }
}
