using ICV.Control.ZoomableImage.ViewModels;
using System;

namespace ICV.Control.ZoomableImage.Views
{
    /// <summary>
    /// 画像の倍率管理
    /// </summary>
    internal readonly struct ImageZoomMagnification
    {
        private static readonly double MagRatioMin = Math.Pow(2, -5);   // 3.1%
        private static readonly double MagRatioMax = Math.Pow(2, 5);    // 3200%
        private static readonly double MagStep = 2.0;                   // 2倍

        public readonly bool IsEntire;
        public readonly double MagnificationRatio;

        public ImageZoomMagnification(in ImageZoomPayload payload)
        {
            if (payload.IsEntire)
            {
                IsEntire = true;
                MagnificationRatio = double.NaN;
            }
            else
            {
                IsEntire = false;
                MagnificationRatio = payload.MagRatio;
            }
        }

        public ImageZoomMagnification(bool flag, double mag = double.NaN)
        {
            IsEntire = flag;
            MagnificationRatio = mag;
        }

        private ImageZoomMagnification(double mag) : this(false, mag) { }

        public static ImageZoomMagnification Entire = new ImageZoomMagnification(true);
        public static ImageZoomMagnification MagX1 = new ImageZoomMagnification(1.0);

        private static ImageZoomMagnification ZoomMagnification(double currentMag, double ratio)
        {
            // ホイールすると2の冪乗になるよう元の倍率を補正する
            double currentMagPowerRaw = Math.Log(currentMag) / Math.Log(2);
            double currentMagRound = Math.Pow(2, Math.Round(currentMagPowerRaw));

            double newMag = currentMagRound * ratio;
            if (newMag < MagRatioMin) newMag = MagRatioMin;
            else if (newMag > MagRatioMax) newMag = MagRatioMax;
            return new ImageZoomMagnification(newMag);
        }

        public ImageZoomMagnification ZoomMagnification(double currentMag, bool isZoomIn)
        {
            var step = isZoomIn ? MagStep : 1.0 / MagStep;
            return ZoomMagnification(currentMag, step);
        }

        public override string ToString() => $"{IsEntire}, {MagnificationRatio:f2}";

    }
}
