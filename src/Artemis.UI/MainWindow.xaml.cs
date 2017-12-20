using MahApps.Metro.Controls;

namespace Artemis.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            AppBootstrapper = new AppBootstrapper();
            DataContext = AppBootstrapper;
        }

        public AppBootstrapper AppBootstrapper { get; }
    }
}