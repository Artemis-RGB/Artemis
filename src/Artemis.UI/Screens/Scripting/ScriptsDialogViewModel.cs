using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.ScriptingProviders;
using Artemis.Core.Services;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens.Scripting.Dialogs;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Artemis.UI.Screens.Scripting;

public class ScriptsDialogViewModel : DialogViewModelBase<object?>
{
    private readonly Dictionary<ScriptingProvider, IScriptEditorViewModel> _providerViewModels = new();
    private readonly IScriptingService _scriptingService;
    private readonly IWindowService _windowService;
    private ObservableAsPropertyHelper<bool>? _hasScripts;
    private ReadOnlyObservableCollection<ScriptConfigurationViewModel> _scriptConfigurations;
    private IScriptEditorViewModel? _scriptEditorViewModel;
    private ScriptConfigurationViewModel? _selectedScript;

    public ScriptsDialogViewModel(IScriptingService scriptingService, IWindowService windowService, IProfileService profileService, IScriptVmFactory scriptVmFactory)
    {
        _scriptingService = scriptingService;
        _windowService = windowService;

        ScriptType = ScriptType.Global;
        ScriptingProviders = new List<ScriptingProvider>(scriptingService.ScriptingProviders);

        AddScriptConfiguration = ReactiveCommand.CreateFromTask(ExecuteAddScriptConfiguration, Observable.Return(ScriptingProviders.Any()));
        this.WhenAnyValue(vm => vm.SelectedScript).Subscribe(s => SetupScriptEditor(s?.ScriptConfiguration));

        _scriptConfigurations = new ReadOnlyObservableCollection<ScriptConfigurationViewModel>(new ObservableCollection<ScriptConfigurationViewModel>());
        // TODO: When not bound to a profile, base the contents of the UI on the ScriptingService
    }

    public ScriptsDialogViewModel(Profile profile, IScriptingService scriptingService, IWindowService windowService, IProfileService profileService, IScriptVmFactory scriptVmFactory)
        : this(scriptingService, windowService, profileService, scriptVmFactory)
    {
        ScriptType = ScriptType.Profile;
        Profile = profile;

        this.WhenActivated(d =>
        {
            _hasScripts = Profile.ScriptConfigurations.ToObservableChangeSet()
                .Count()
                .Select(c => c > 0)
                .ToProperty(this, vm => vm.HasScripts)
                .DisposeWith(d);
            Profile.ScriptConfigurations.ToObservableChangeSet()
                .Transform(c => scriptVmFactory.ScriptConfigurationViewModel(Profile, c))
                .Bind(out ReadOnlyObservableCollection<ScriptConfigurationViewModel> scriptConfigurationViewModels)
                .Subscribe()
                .DisposeWith(d);

            ScriptConfigurations = scriptConfigurationViewModels;
            SelectedScript = ScriptConfigurations.FirstOrDefault();
            Disposable.Create(() => profileService.SaveProfile(Profile, false)).DisposeWith(d);
        });
    }

    public ScriptType ScriptType { get; }
    public List<ScriptingProvider> ScriptingProviders { get; }
    public Profile? Profile { get; }
    public bool HasScripts => _hasScripts?.Value ?? false;

    public ReadOnlyObservableCollection<ScriptConfigurationViewModel> ScriptConfigurations
    {
        get => _scriptConfigurations;
        set => RaiseAndSetIfChanged(ref _scriptConfigurations, value);
    }

    public ScriptConfigurationViewModel? SelectedScript
    {
        get => _selectedScript;
        set => RaiseAndSetIfChanged(ref _selectedScript, value);
    }

    public IScriptEditorViewModel? ScriptEditorViewModel
    {
        get => _scriptEditorViewModel;
        set => RaiseAndSetIfChanged(ref _scriptEditorViewModel, value);
    }

    public ReactiveCommand<Unit, Unit> AddScriptConfiguration { get; }

    public async Task<bool> CanClose()
    {
        if (!ScriptConfigurations.Any(s => s.ScriptConfiguration.HasChanges))
            return true;

        bool result = await _windowService.ShowConfirmContentDialog("Discard changes", "One or more scripts still have pending changes, do you want to discard them?");
        if (!result)
            return false;

        foreach (ScriptConfigurationViewModel scriptConfigurationViewModel in ScriptConfigurations)
            scriptConfigurationViewModel.ScriptConfiguration.DiscardPendingChanges();
        return true;
    }


    private void SetupScriptEditor(ScriptConfiguration? scriptConfiguration)
    {
        if (scriptConfiguration == null)
        {
            ScriptEditorViewModel = null;
            return;
        }

        // The script is null if the provider is missing
        if (scriptConfiguration.Script == null)
        {
            ScriptEditorViewModel = null;
            return;
        }

        if (!_providerViewModels.TryGetValue(scriptConfiguration.Script.ScriptingProvider, out IScriptEditorViewModel? viewModel))
        {
            viewModel = scriptConfiguration.Script.ScriptingProvider.CreateScriptEditor(ScriptType);
            _providerViewModels.Add(scriptConfiguration.Script.ScriptingProvider, viewModel);
        }

        ScriptEditorViewModel = viewModel;
        ScriptEditorViewModel.ChangeScript(scriptConfiguration.Script);
    }

    private async Task ExecuteAddScriptConfiguration()
    {
        await _windowService.CreateContentDialog()
            .WithTitle("Add script")
            .WithViewModel(out ScriptConfigurationCreateViewModel vm)
            .WithCloseButtonText("Cancel")
            .HavingPrimaryButton(b => b.WithText("Confirm").WithCommand(vm.Submit))
            .WithDefaultButton(ContentDialogButton.Primary)
            .ShowAsync();

        if (vm.ScriptConfiguration == null)
            return;

        // Add the script to the profile and instantiate it
        if (Profile != null)
            _scriptingService.AddScript(vm.ScriptConfiguration, Profile);
        else
            _scriptingService.AddScript(vm.ScriptConfiguration);

        // Select the new script
        SelectedScript = ScriptConfigurations.LastOrDefault();
    }
}