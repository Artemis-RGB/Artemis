using Artemis.UI.Avalonia.Screens.Root.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Root.Views
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