using System.Reactive;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.ScriptingProviders;
using Artemis.Core.Services;
using Artemis.UI.Screens.Scripting.Dialogs;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ContentDialogButton = Artemis.UI.Shared.Services.Builders.ContentDialogButton;

namespace Artemis.UI.Screens.Scripting;

public class ScriptConfigurationViewModel : ActivatableViewModelBase
{
    private readonly IScriptingService _scriptingService;
    private readonly IWindowService _windowService;

    public ScriptConfigurationViewModel(ScriptConfiguration scriptConfiguration, IScriptingService scriptingService, IWindowService windowService)
    {
        _scriptingService = scriptingService;
        _windowService = windowService;

        ScriptConfiguration = scriptConfiguration;
        Script = ScriptConfiguration.Script;
        EditScriptConfiguration = ReactiveCommand.CreateFromTask<ScriptConfiguration>(ExecuteEditScriptConfiguration);
        ToggleSuspended = ReactiveCommand.Create(() => ScriptConfiguration.IsSuspended = !ScriptConfiguration.IsSuspended);
    }

    public ScriptConfigurationViewModel(Profile profile, ScriptConfiguration scriptConfiguration, IScriptingService scriptingService, IWindowService windowService)
        : this(scriptConfiguration, scriptingService, windowService)
    {
        Profile = profile;
    }

    public Profile? Profile { get; }
    public ScriptConfiguration ScriptConfiguration { get; }
    public Script? Script { get; }
    public ReactiveCommand<ScriptConfiguration, Unit> EditScriptConfiguration { get; }
    public ReactiveCommand<Unit, bool> ToggleSuspended { get; }

    private async Task ExecuteEditScriptConfiguration(ScriptConfiguration scriptConfiguration)
    {
        ContentDialogResult contentDialogResult = await _windowService.CreateContentDialog()
            .WithTitle("Edit script")
            .WithViewModel(out ScriptConfigurationEditViewModel vm, scriptConfiguration)
            .WithCloseButtonText("Cancel")
            .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Submit))
            .HavingSecondaryButton(b => b.WithText("Delete"))
            .WithDefaultButton(ContentDialogButton.Primary)
            .ShowAsync();

        // Remove the script if the delete button was pressed
        if (contentDialogResult == ContentDialogResult.Secondary)
        {
            if (Profile != null)
                _scriptingService.RemoveScript(scriptConfiguration, Profile);
            else
                _scriptingService.RemoveScript(scriptConfiguration);
        }
    }
}