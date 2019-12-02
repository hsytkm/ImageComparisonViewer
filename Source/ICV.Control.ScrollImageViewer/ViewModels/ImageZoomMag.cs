using System;

namespace ICV.Control.ScrollImageViewer.ViewModels
{
    public readonly struct ImageZoomMag : IEquatable<ImageZoomMag>
    {
        private static readonly double _magRatioMin = Math.Pow(2, -6);  // 1.5%
        private static readonly double _magRatioMax = Math.Pow(2, 7);   // 12800%
        private const double _magStep = 2.0;                            // 2倍

        /// <summary>全体表示フラグ</summary>
        public readonly bool IsEntire;

        /// <summary>ズーム倍率(等倍=1.0)</summary>
        public readonly double MagRatio;

        public static readonly ImageZoomMag Entire = new ImageZoomMag(true, double.NaN);
        public static readonly ImageZoomMag MagX1 = new ImageZoomMag(false, 1.0);

        public ImageZoomMag(bool entire, double mag)
        {
            (IsEntire, MagRatio) = (entire, mag);
        }

        private static ImageZoomMag ZoomMagnification(double currentMag, double ratio)
        {
            static double clip(double value, double min, double max) => (value <= min) ? min : ((value >= max) ? max : value);

            // ホイールすると2の冪乗になるよう元の倍率を補正する
            double currentMagPowerRaw = Math.Log(currentMag) / Math.Log(2);
            double currentMagRound = Math.Pow(2, Math.Round(currentMagPowerRaw));

            double newMag = clip(currentMagRound * ratio, _magRatioMin, _magRatioMax);
            return new ImageZoomMag(false, newMag);
        }

        public static ImageZoomMag ZoomMagnification(double currentMag, bool isZoomIn)
        {
            var step = isZoomIn ? _magStep : (1 / _magStep);
            return ZoomMagnification(currentMag, step);
        }

        #region IEquatable<T>
        public bool Equals(ImageZoomMag other)
        {
            return IsEntire == other.IsEntire && MagRatio == other.MagRatio;
        }

        public override bool Equals(object? other)
        {
            if (other is ImageZoomMag)
                return Equals((ImageZoomMag)other);
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(IsEntire, MagRatio);
        }

        public static bool operator ==(ImageZoomMag left, ImageZoomMag right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ImageZoomMag left, ImageZoomMag right)
        {
            return !(left == right);
        }
        #endregion

        public override string ToString() =>
            $"{nameof(IsEntire)}={IsEntire}, {nameof(MagRatio)}={MagRatio:f3}";
    }
}
