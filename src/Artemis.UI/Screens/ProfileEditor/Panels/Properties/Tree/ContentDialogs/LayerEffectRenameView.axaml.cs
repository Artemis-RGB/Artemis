using System.Threading.Tasks;
using Artemis.UI.Shared.Extensions;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Tree.ContentDialogs;

public partial class LayerEffectRenameView : ReactiveUserControl<LayerEffectRenameViewModel>
{
    public LayerEffectRenameView()
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

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}