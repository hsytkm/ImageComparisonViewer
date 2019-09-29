namespace ImageComparisonViewer.MainTabControl.ViewModels.Bases
{
    abstract class TabContentImageViewModelBase : TabContentViewModelBase
    {
        public int Index { get; }

        public TabContentImageViewModelBase(string title, int index)
            : base (title)
        {
            Index = index;
        }
    }
}
