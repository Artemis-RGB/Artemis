using Artemis.UI.Screens.Workshop.ViewModels;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Views
{
    public class WorkshopView : ReactiveUserControl<WorkshopViewModel>
    {
        public WorkshopView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}