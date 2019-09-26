using ImageComparisonViewer.MainTabControl.ViewModels.Bases;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;

namespace ImageComparisonViewer.MainTabControl.ViewModels
{
    class TabContentDoubleViewModel : TabContentImageViewModelBase
    {
        private readonly static string _title = "Double";
        private readonly static int _index = 2;

        /// <summary>
        /// 画像Drop時のイベント通知
        /// </summary>
        public ReactiveProperty<IReadOnlyList<string>> DropEvent { get; } =
            new ReactiveProperty<IReadOnlyList<string>>(mode: ReactivePropertyMode.None);

        public TabContentDoubleViewModel() : base(_title, _index)
        {
            Debug.WriteLine($"{nameof(TabContentDoubleViewModel): ctor}");

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
