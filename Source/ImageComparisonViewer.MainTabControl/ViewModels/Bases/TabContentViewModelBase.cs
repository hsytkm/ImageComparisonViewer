using ImageComparisonViewer.Common.Mvvm;
using Prism;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Diagnostics;

namespace ImageComparisonViewer.MainTabControl.ViewModels.Bases
{
    abstract class TabContentViewModelBase : DisposableBindableBase, IActiveAware, INavigationAware
    {
        public string Title { get; }

        public TabContentViewModelBase(string title)
        {
            Title = title;

            IsActiveChanged += (sender, e) =>
            {
                if (e is DataEventArgs<bool> e2)
                    Debug.WriteLine($"{Title}-IsActive: {e2.Value}");
            };
        }

        #region IActiveAware

        public event EventHandler IsActiveChanged;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                    IsActiveChanged?.Invoke(this, new DataEventArgs<bool>(value));
            }
        }
        private bool _isActive;

        #endregion

        #region INavigationAware

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Debug.WriteLine($"OnNavigatedFrom({Title})");
        }

        // ナビゲーションが移ってきた時にコールされる(UI操作で移行した場合は来ない)
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Debug.WriteLine($"OnNavigatedTo({Title})");

            //var messages = new List<string>();
            //for (int i = 0; i < Index; i++)
            //{
            //    if (navigationContext.Parameters[$"image{i}"] is string message)
            //        messages.Add(message);
            //}

            //if (messages.Any())
            //    Message = string.Join(" | ", messages);
        }

        //public static NavigationParameters GetNavigationParameters(IList<string> messages)
        //{
        //    var parameters = new NavigationParameters();
        //    for (int i = 0; i < messages.Count; i++)
        //    {
        //        parameters.Add($"image{i}", messages[i]);
        //    }
        //    return parameters;
        //}

        #endregion

    }
}
