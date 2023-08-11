using System;
using System.Reactive;
using System.Reactive.Disposables;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;
using Artemis.WebClient.Workshop;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class EntrySpecificationsStepViewModel : SubmissionViewModel
{
    private string _name = string.Empty;
    private string _summary = string.Empty;
    private string _description = string.Empty;

    public EntrySpecificationsStepViewModel()
    {
        GoBack = ReactiveCommand.Create(ExecuteGoBack);
        Continue = ReactiveCommand.Create(ExecuteContinue, ValidationContext.Valid);
       
        this.WhenActivated((CompositeDisposable _) =>
        {
            this.ClearValidationRules();
            
            DisplayName = $"{State.EntryType} Information";
            Name = State.Name;
            Summary = State.Summary;
            Description = State.Description;
        });
    }
   
    public override ReactiveCommand<Unit, Unit> Continue { get; }
    public override ReactiveCommand<Unit, Unit> GoBack { get; }

    public string Name
    {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string Summary
    {
        get => _summary;
        set => RaiseAndSetIfChanged(ref _summary, value);
    }

    public string Description
    {
        get => _description;
        set => RaiseAndSetIfChanged(ref _description, value);
    }

    private void ExecuteGoBack()
    {
        switch (State.EntryType)
        {
            case EntryType.Layout:
                break;
            case EntryType.Plugin:
                break;
            case EntryType.Profile:
                State.ChangeScreen<ProfileAdaptionHintsStepViewModel>();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void ExecuteContinue()
    {
        this.ValidationRule(vm => vm.Name, s => !string.IsNullOrWhiteSpace(s), "Name cannot be empty.");
        this.ValidationRule(vm => vm.Summary, s => !string.IsNullOrWhiteSpace(s), "Summary cannot be empty.");
        this.ValidationRule(vm => vm.Description, s => !string.IsNullOrWhiteSpace(s), "Description cannot be empty.");
        
        if (!ValidationContext.IsValid)
            return;
        
        State.Name = Name;
        State.Summary = Summary;
        State.Description = Description;
    }
}