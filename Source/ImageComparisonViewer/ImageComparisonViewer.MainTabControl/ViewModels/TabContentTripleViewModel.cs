using ImageComparisonViewer.MainTabControl.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class TabContentTripleViewModel : TabContentImageViewModelBase
    {
        private readonly static string _title = "Triple";
        private readonly static int _index = 3;

        /// <summary>
        /// 画像Drop時のイベント通知
        /// </summary>
        public ReactiveProperty<IReadOnlyList<string>> DropEvent { get; } =
            new ReactiveProperty<IReadOnlyList<string>>(mode: ReactivePropertyMode.None);

        public TabContentTripleViewModel() : base(_title, _index)
        {
            Debug.WriteLine($"{nameof(TabContentTripleViewModel): ctor}");

            DropEvent
                .Subscribe(paths =>
                {
                    foreach (var path in paths)
                    {
                        Debug.WriteLine(path);
                    }
                })
                .AddTo(CompositeDisposable);

        }

    }
}
