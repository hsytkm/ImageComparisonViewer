using Control.ImagePanel.ViewModels;
using System.Windows.Controls;

namespace Control.ImagePanel.Views
{
    /// <summary>
    /// Interaction logic for ImagePanel.xaml
    /// </summary>
    public partial class ImagePanel : UserControl
    {
        private readonly int _contentIndex;


        public ImagePanel(int contentIndex)
        {
            InitializeComponent();

            _contentIndex = contentIndex;

            // ◆ViewModelのコンストラクタに引数渡したい…
            UpdateImageSource();

        }

        public void UpdateImageSource()
        {
            if (DataContext is ImagePanelViewModel vmodel)
            {
                vmodel.SetContentIndex(_contentIndex);
            }
        }

    }
}