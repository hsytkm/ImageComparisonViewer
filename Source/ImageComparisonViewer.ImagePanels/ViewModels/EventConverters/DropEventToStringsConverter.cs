using Reactive.Bindings.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ImageComparisonViewer.ImagePanels.ViewModels.EventConverters
{
    /// <summary>
    /// Dropイベント(dynamicはDragEventArgs想定)
    /// </summary>
    class DropEventToStringsConverter : ReactiveConverter<dynamic, IReadOnlyList<string>>
    {
        protected override IObservable<IReadOnlyList<string>> OnConvert(IObservable<dynamic> source)
        {
            return source
                .Cast<DragEventArgs>()
                .Select(e => ToStrings(e.Data));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IReadOnlyList<string> ToStrings(IDataObject data)
        {
            if (data.GetDataPresent(DataFormats.FileDrop))
            {
                if (data.GetData(DataFormats.FileDrop) is string[] list)
                    return list;
            }
            else
            {
                var str = data.GetData(DataFormats.Text)?.ToString();
                if (str != null)
                    return new List<string>() { str };
            }
            return Enumerable.Empty<string>().ToList();
        }

    }
}
