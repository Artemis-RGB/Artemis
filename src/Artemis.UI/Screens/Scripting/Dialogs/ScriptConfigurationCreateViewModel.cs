using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Artemis.Core.ScriptingProviders;
using Artemis.Core.Services;
using Artemis.UI.Shared;
using FluentAvalonia.UI.Controls;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Scripting.Dialogs;

public partial class ScriptConfigurationCreateViewModel : ContentDialogViewModelBase
{
    [Notify] private string? _scriptName;
    [Notify] private ScriptingProvider _selectedScriptingProvider;

    public ScriptConfigurationCreateViewModel(IScriptingService scriptingService)
    {
        ScriptingProviders = new List<ScriptingProvider>(scriptingService.ScriptingProviders);
        Submit = ReactiveCommand.Create(ExecuteSubmit, ValidationContext.Valid);
        _selectedScriptingProvider = ScriptingProviders.First();

        this.ValidationRule(vm => vm.ScriptName, s => !string.IsNullOrWhiteSpace(s), "Script name cannot be empty.");
    }

    public ScriptConfiguration? ScriptConfiguration { get; private set; }
    public List<ScriptingProvider> ScriptingProviders { get; }
    public ReactiveCommand<Unit, Unit> Submit { get; }

    private void ExecuteSubmit()
    {
        if (ScriptName == null)
            return;

        ScriptConfiguration = new ScriptConfiguration(SelectedScriptingProvider, ScriptName, ScriptType.Profile);
        ContentDialog?.Hide(ContentDialogResult.Primary);
    }
}