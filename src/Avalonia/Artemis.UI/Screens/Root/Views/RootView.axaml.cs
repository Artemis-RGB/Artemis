using Artemis.UI.Screens.Root.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Root.Views
{
    public class RootView : ReactiveUserControl<RootViewModel>
    {
        public RootView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}