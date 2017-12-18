using System.Windows.Controls;
using Artemis.UI.ViewModels;

namespace Artemis.UI.Views
{
    /// <summary>
    ///     Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            ViewModel = new MainViewModel();
            InitializeComponent();
            DataContext = ViewModel;
        }

        public MainViewModel ViewModel { get; set; }
    }
}
