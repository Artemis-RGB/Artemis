using Artemis.UI.Avalonia.Screens.Settings.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Settings.Views
{
    public partial class AboutTabView : ReactiveUserControl<AboutTabViewModel>
    {
        public AboutTabView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
