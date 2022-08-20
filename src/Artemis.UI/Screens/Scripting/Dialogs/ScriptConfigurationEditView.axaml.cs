using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Screens.Scripting.Dialogs;

public partial class ScriptConfigurationEditView : ReactiveUserControl<ScriptConfigurationEditViewModel>
{
    public ScriptConfigurationEditView()
    {
        InitializeComponent();
        this.WhenActivated(_ =>
        {
            this.Get<TextBox>("Input").Focus();
            this.Get<TextBox>("Input").SelectAll();
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}