using Prism.Common;
using Prism.Regions;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace ImageComparisonViewer.Common.Prism
{
    public class DisposeBehavior : IRegionBehavior
    {
        public const string Key = nameof(DisposeBehavior);
        public IRegion Region { get; set; } = default!;

        public void Attach()
        {
            Region.Views.CollectionChanged += Views_CollectionChanged;

            var n = Region.Name;
            var a = Region.Views.ToArray();
        }

        private void Views_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            static void callDispose(IDisposable d) => d.Dispose();

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var o in e.OldItems)
                {
                    MvvmHelpers.ViewAndViewModelAction(o, (Action<IDisposable>)callDispose);
                }
            }
        }

    }
}
