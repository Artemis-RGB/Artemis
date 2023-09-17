using System.Threading.Tasks;
using Artemis.UI.Shared.Extensions;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public partial class SidebarCategoryEditView : ReactiveUserControl<SidebarCategoryEditViewModel>
{
    public SidebarCategoryEditView()
    {
        InitializeComponent();
        this.WhenActivated(_ =>
        {
            this.ClearAllDataValidationErrors();
            Dispatcher.UIThread.Post(DelayedAutoFocus);
        });
    }
    
    private async void DelayedAutoFocus()
    {
        // Don't ask
        await Task.Delay(200);
        NameTextBox.SelectAll();
        NameTextBox.Focus();
    }
}