using System.Threading.Tasks;
using Artemis.UI.Shared.Extensions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileTree.ContentDialogs;

public partial class ProfileElementRenameView : ReactiveUserControl<ProfileElementRenameViewModel>
{
    public ProfileElementRenameView()
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