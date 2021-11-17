using Artemis.UI.Screens.Root.ViewModels;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.Root.Views
{
    public class SidebarCategoryView : ReactiveUserControl<SidebarCategoryViewModel>
    {
        public SidebarCategoryView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void Title_OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (ViewModel != null)
                ViewModel.ShowItems = !ViewModel.ShowItems;
        }
    }
}