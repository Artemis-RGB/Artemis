using Artemis.UI.Avalonia.Screens.Home.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Home.Views
{
    public class HomeView : ReactiveUserControl<HomeViewModel>
    {
        public HomeView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}