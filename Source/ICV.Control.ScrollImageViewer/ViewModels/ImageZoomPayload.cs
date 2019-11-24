using System;

namespace ICV.Control.ScrollImageViewer.ViewModels
{
    public readonly struct ImageZoomPayload : IEquatable<ImageZoomPayload>
    {
        private static readonly double _magRatioMin = Math.Pow(2, -5);   // 3.1%
        private static readonly double _magRatioMax = Math.Pow(2, 7);    // 12800%
        private static readonly double _magStep = 2.0;                   // 2倍

        /// <summary>全体表示フラグ</summary>
        public readonly bool IsEntire;

        /// <summary>ズーム倍率(等倍=1.0)</summary>
        public readonly double MagRatio;

        public ImageZoomPayload(bool entire, double mag)
        {
            (IsEntire, MagRatio) = (entire, mag);
        }

        private static ImageZoomPayload ZoomMagnification(double currentMag, double ratio)
        {
            static double clip(double value, double min, double max) => (value <= min) ? min : ((value >= max) ? max : value);

            // ホイールすると2の冪乗になるよう元の倍率を補正する
            double currentMagPowerRaw = Math.Log(currentMag) / Math.Log(2);
            double currentMagRound = Math.Pow(2, Math.Round(currentMagPowerRaw));

            double newMag = clip(currentMagRound * ratio, _magRatioMin, _magRatioMax);
            return new ImageZoomPayload(false, newMag);
        }

        public ImageZoomPayload ZoomMagnification(double currentMag, bool isZoomIn)
        {
            var step = isZoomIn ? _magStep : (1 / _magStep);
            return ZoomMagnification(currentMag, step);
        }

        #region IEquatable<T>
        public bool Equals(ImageZoomPayload other)
        {
            return IsEntire == other.IsEntire && MagRatio == other.MagRatio;
        }

        public override bool Equals(object? other)
        {
            if (other is ImageZoomPayload)
                return Equals((ImageZoomPayload)other);
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsEntire, MagRatio);
        }
        #endregion

    }
}
