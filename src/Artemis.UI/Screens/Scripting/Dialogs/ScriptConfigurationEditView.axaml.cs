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
            Input.Focus();
            Input.SelectAll();
        });
    }

}