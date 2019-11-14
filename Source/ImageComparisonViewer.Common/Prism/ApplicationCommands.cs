using Prism.Commands;

namespace ImageComparisonViewer.Common.Prism
{
    public interface IApplicationCommands
    {
        CompositeCommand ImagesRightShiftCommand { get; }
        CompositeCommand ImagesLeftShiftCommand { get; }
        CompositeCommand SelectNextImageCommand { get; }
        CompositeCommand SelectPrevImageCommand { get; }
    }

    public class ApplicationCommands : IApplicationCommands
    {
        public CompositeCommand ImagesRightShiftCommand { get; } = new CompositeCommand();
        public CompositeCommand ImagesLeftShiftCommand { get; } = new CompositeCommand();

        public CompositeCommand SelectNextImageCommand { get; } = new CompositeCommand();
        public CompositeCommand SelectPrevImageCommand { get; } = new CompositeCommand();

    }
}
