using System;
using System.Collections.Generic;
using System.Text;

namespace ImageComparisonViewer.Common.Extensions
{
    public static class ICollectionExtension
    {
        public static void ClearWithDispose<T>(this ICollection<T> collection)
            where T : IDisposable
        {
            if (collection is null) return;

            foreach (var item in collection)
            {
                item.Dispose();
            }
            collection.Clear();
        }
    }
}
