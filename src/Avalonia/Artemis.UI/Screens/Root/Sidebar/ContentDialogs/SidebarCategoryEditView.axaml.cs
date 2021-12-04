using Artemis.UI.Shared.Extensions;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.Root.Sidebar.Dialogs
{
    public class SidebarCategoryEditView : ReactiveUserControl<SidebarCategoryEditViewModel>
    {
        public SidebarCategoryEditView()
        {
            InitializeComponent();
            this.WhenActivated(_ => this.ClearAllDataValidationErrors());
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}