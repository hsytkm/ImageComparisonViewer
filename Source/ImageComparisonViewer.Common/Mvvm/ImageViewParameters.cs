using System;
using System.Collections.Generic;

namespace ImageComparisonViewer.Common.Mvvm
{
    /// <summary>
    /// ImagePanelのViewから引き回されるパラメータ
    /// </summary>
    public readonly struct ImageViewParameter
    {
        public readonly int ContentIndex;
        public readonly int ContentCount;

        public ImageViewParameter(int index, int count) =>
            (ContentIndex, ContentCount) = (index, count);
    }

    /// <summary>
    /// ボックス化削減対応(作成済みobjectを使い回す)
    /// </summary>
    public static class ImageViewParameterFactory
    {
        /// <summary>
        /// パラメータ辞書(保持して効率化)
        /// </summary>
        private static readonly IDictionary<long, (Type, object)[]> _parametersMap =
            new Dictionary<long, (Type, object)[]>();

        /// <summary>
        /// ImagePanelViewに伝搬させるコンテナ用パラメータ
        /// </summary>
        /// <param name="contentIndex">Imageインデックス(最大3なら0~2)</param>
        /// <param name="contentCount">View内のImage最大数</param>
        /// <returns></returns>
        public static (Type, object)[] GetImageViewParameters(int contentIndex, int contentCount)
        {
            static long CalcKey(int x, int y) => (((long)x) << 32) | ((long)y);

            var key = CalcKey(contentIndex, contentCount);
            if (_parametersMap.TryGetValue(key, out var value))
                return value;

            var param = new ImageViewParameter(contentIndex, contentCount);
            var parameters = new[] { (typeof(ImageViewParameter), (object)param) };
            _parametersMap.Add(key, parameters);
            return parameters;
        }

        /// <summary>
        /// ImagePanelViewに伝搬させるコンテナ用パラメータ
        /// </summary>
        /// <param name="param">パラメータ</param>
        /// <returns></returns>
        public static (Type, object)[] GetImageViewParameters(ImageViewParameter param) =>
            GetImageViewParameters(param.ContentIndex, param.ContentCount);

    }

}
