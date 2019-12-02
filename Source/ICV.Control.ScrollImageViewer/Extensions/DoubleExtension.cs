using System;

namespace ICV.Control.ScrollImageViewer.Extensions
{
    static class DoubleExtension
    {
        /// <summary>doubleの値がほぼ同じかチェック</summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="tolerancePoint">許容度(2なら小数第二位までチェック)</param>
        /// <returns></returns>
        public static bool AreClose(this double value1, double value2, int tolerancePoint)
        {
            if (value1 == value2) return true;

            var ratio = Math.Pow(10, tolerancePoint);
            var diff = Math.Round(value1 * ratio) - Math.Round(value2 * ratio);
            return diff == 0;
        }

    }
}
