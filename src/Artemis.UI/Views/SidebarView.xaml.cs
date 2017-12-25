using System.Windows;
using System.Windows.Controls;
using Artemis.UI.ViewModels;
using ReactiveUI;

namespace Artemis.UI.Views
{
    /// <summary>
    ///     Interaction logic for SidebarView.xaml
    /// </summary>
    public partial class SidebarView : UserControl, IViewFor<ISidebarViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(ISidebarViewModel), typeof(SidebarView), new PropertyMetadata(null));

        public SidebarView()
        {
            InitializeComponent();
            this.WhenAnyValue(x => x.ViewModel).BindTo(this, x => x.DataContext);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ISidebarViewModel)value;
        }

        public ISidebarViewModel ViewModel { get; set; }
    }
}