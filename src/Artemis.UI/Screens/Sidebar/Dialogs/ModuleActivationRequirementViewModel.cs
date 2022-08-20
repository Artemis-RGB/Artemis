using System;
using System.Reactive.Disposables;
using Artemis.Core.Modules;
using Artemis.UI.Shared;
using Avalonia.Threading;
using Humanizer;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public class ModuleActivationRequirementViewModel : ActivatableViewModelBase
{
    private readonly IModuleActivationRequirement _activationRequirement;
    private string _requirementDescription;
    private bool _requirementMet;
    private DispatcherTimer? _updateTimer;

    public ModuleActivationRequirementViewModel(IModuleActivationRequirement activationRequirement)
    {
        RequirementName = activationRequirement.GetType().Name.Humanize();
        _requirementDescription = activationRequirement.GetUserFriendlyDescription();
        _activationRequirement = activationRequirement;

        this.WhenActivated(d =>
        {
            _updateTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Normal, Update);
            _updateTimer.Start();

            Disposable.Create(() =>
            {
                _updateTimer?.Stop();
                _updateTimer = null;
            }).DisposeWith(d);
        });
    }

    public string RequirementName { get; }

    public string RequirementDescription
    {
        get => _requirementDescription;
        set => RaiseAndSetIfChanged(ref _requirementDescription, value);
    }

    public bool RequirementMet
    {
        get => _requirementMet;
        set => RaiseAndSetIfChanged(ref _requirementMet, value);
    }

    private void Update(object? sender, EventArgs e)
    {
        RequirementDescription = _activationRequirement.GetUserFriendlyDescription();
        RequirementMet = _activationRequirement.Evaluate();
    }
}