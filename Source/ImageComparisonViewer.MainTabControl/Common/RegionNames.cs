using ImageComparisonViewer.MainTabControl.Views;
using System;
using System.Collections.Generic;

namespace ImageComparisonViewer.MainTabControl.Common
{
    static class RegionNames
    {
        internal static string GetTabContentName(int index)
        {
            return index switch
            {
                1 => nameof(TabContentSingle),
                2 => nameof(TabContentDouble),
                3 => nameof(TabContentTriple),
                _ => throw new ArgumentOutOfRangeException(index.ToString())
            };
        }

        private static string GetImageContentRegionName(int count, int index)
        {
            if (count <= index) throw new ArgumentException("index over count");
            return $"ImageContentRegion{count}_{index}";
        }

        /// <summary>
        /// 指定Indexの画像Region名を昇順に返す
        /// </summary>
        /// <param name="contentCount"></param>
        /// <returns></returns>
        internal static IEnumerable<string> GetImageContentRegionNames(int contentCount)
        {
            for (int index = 0; index < contentCount; index++)
                yield return GetImageContentRegionName(contentCount, index);
        }

    }
}
