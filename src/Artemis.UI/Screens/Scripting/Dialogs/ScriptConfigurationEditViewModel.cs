using System.Reactive;
using Artemis.Core.ScriptingProviders;
using Artemis.UI.Shared;
using FluentAvalonia.UI.Controls;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Scripting.Dialogs;

public partial class ScriptConfigurationEditViewModel : ContentDialogViewModelBase
{
    [Notify] private string? _scriptName;

    public ScriptConfigurationEditViewModel(ScriptConfiguration scriptConfiguration)
    {
        ScriptConfiguration = scriptConfiguration;
        Submit = ReactiveCommand.Create(ExecuteSubmit, ValidationContext.Valid);
        ScriptName = ScriptConfiguration.Name;

        this.ValidationRule(vm => vm.ScriptName, s => !string.IsNullOrWhiteSpace(s), "Script name cannot be empty.");
    }

    public ScriptConfiguration ScriptConfiguration { get; }
    public ReactiveCommand<Unit, Unit> Submit { get; }

    private void ExecuteSubmit()
    {
        if (ScriptName == null)
            return;

        ScriptConfiguration.Name = ScriptName;
        ContentDialog?.Hide(ContentDialogResult.Primary);
    }
}