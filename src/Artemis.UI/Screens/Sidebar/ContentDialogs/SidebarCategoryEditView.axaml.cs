using Artemis.UI.Shared.Extensions;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public partial class SidebarCategoryEditView : ReactiveUserControl<SidebarCategoryEditViewModel>
{
    public SidebarCategoryEditView()
    {
        InitializeComponent();
        this.WhenActivated(_ => this.ClearAllDataValidationErrors());
    }

}