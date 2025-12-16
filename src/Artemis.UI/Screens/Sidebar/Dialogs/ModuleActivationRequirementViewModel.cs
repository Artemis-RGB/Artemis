using System;
using System.Reactive.Disposables.Fluent;
using System.Timers;
using Artemis.Core.Modules;
using Artemis.UI.Shared;
using Humanizer;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Sidebar;

public partial class ModuleActivationRequirementViewModel : ActivatableViewModelBase
{
    private readonly IModuleActivationRequirement _activationRequirement;
    [Notify] private string _requirementDescription;
    [Notify] private bool _requirementMet;

    public ModuleActivationRequirementViewModel(IModuleActivationRequirement activationRequirement)
    {
        RequirementName = activationRequirement.GetType().Name.Humanize();
        _requirementDescription = activationRequirement.GetUserFriendlyDescription();
        _activationRequirement = activationRequirement;

        this.WhenActivated(d =>
        {
            Timer updateTimer = new(TimeSpan.FromMilliseconds(500));
            updateTimer.Elapsed += (_, _) => Update();
            updateTimer.Start();
            updateTimer.DisposeWith(d);
        });
    }

    public string RequirementName { get; }

    private void Update()
    {
        RequirementDescription = _activationRequirement.GetUserFriendlyDescription();
        RequirementMet = _activationRequirement.Evaluate();
    }
}