using System;
using System.Collections.ObjectModel;

namespace ImageComparisonViewer.Common.Extensions
{
    public static class ObservableCollectionExtension
    {
        public static void ClearWithDispose<T>(this ObservableCollection<T> collection)
        {
            foreach (var item in collection)
            {
                if (item is IDisposable d)
                    d.Dispose();
            }
            collection.Clear();
        }

    }
}
