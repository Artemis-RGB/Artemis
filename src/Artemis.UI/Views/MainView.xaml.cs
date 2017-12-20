using System.Windows;
using System.Windows.Controls;
using Artemis.UI.ViewModels;
using ReactiveUI;

namespace Artemis.UI.Views
{
    /// <summary>
    ///     Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl, IViewFor<IMainViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(IMainViewModel), typeof(MainView), new PropertyMetadata(null));

        public MainView()
        {
            InitializeComponent();
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (IMainViewModel) value;
        }

        public IMainViewModel ViewModel { get; set; }
    }
}