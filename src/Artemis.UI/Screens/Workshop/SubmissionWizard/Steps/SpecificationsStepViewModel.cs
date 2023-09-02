using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.Entries;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;
using Artemis.WebClient.Workshop;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public class SpecificationsStepViewModel : SubmissionViewModel
{
    public SpecificationsStepViewModel(EntrySpecificationsViewModel entrySpecificationsViewModel)
    {
        EntrySpecificationsViewModel = entrySpecificationsViewModel;
        GoBack = ReactiveCommand.Create(ExecuteGoBack);
        Continue = ReactiveCommand.Create(ExecuteContinue, EntrySpecificationsViewModel.ValidationContext.Valid);

        this.WhenActivated((CompositeDisposable d) =>
        {
            DisplayName = $"{State.EntryType} Information";

            // Apply the state
            ApplyFromState();

            EntrySpecificationsViewModel.ClearValidationRules();
        });
    }

    public EntrySpecificationsViewModel EntrySpecificationsViewModel { get; }
    public override ReactiveCommand<Unit, Unit> Continue { get; }
    public override ReactiveCommand<Unit, Unit> GoBack { get; }

    private void ExecuteGoBack()
    {
        // Apply what's there so far
        ApplyToState();

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
        if (!EntrySpecificationsViewModel.ValidationContext.Validations.Any())
        {
            // The ValidationContext seems to update asynchronously, so stop and schedule a retry
            EntrySpecificationsViewModel.SetupDataValidation();
            Dispatcher.UIThread.Post(ExecuteContinue);
            return;
        }

        ApplyToState();

        if (!EntrySpecificationsViewModel.ValidationContext.GetIsValid())
            return;

        State.ChangeScreen<SubmitStepViewModel>();
    }

    private void ApplyFromState()
    {
        // Basic fields
        EntrySpecificationsViewModel.Name = State.Name;
        EntrySpecificationsViewModel.Summary = State.Summary;
        EntrySpecificationsViewModel.Description = State.Description;

        // Tags
        EntrySpecificationsViewModel.Tags.Clear();
        EntrySpecificationsViewModel.Tags.AddRange(State.Tags);

        // Categories
        EntrySpecificationsViewModel.PreselectedCategories = State.Categories;

        // Icon
        if (State.Icon != null)
        {
            State.Icon.Seek(0, SeekOrigin.Begin);
            EntrySpecificationsViewModel.IconBitmap = BitmapExtensions.LoadAndResize(State.Icon, 128);
        }
    }

    private void ApplyToState()
    {
        // Basic fields
        State.Name = EntrySpecificationsViewModel.Name;
        State.Summary = EntrySpecificationsViewModel.Summary;
        State.Description = EntrySpecificationsViewModel.Description;

        // Categories and tasks
        State.Categories = EntrySpecificationsViewModel.Categories.Where(c => c.IsSelected).Select(c => c.Id).ToList();
        State.Tags = new List<string>(EntrySpecificationsViewModel.Tags);

        // Icon
        State.Icon?.Dispose();
        if (EntrySpecificationsViewModel.IconBitmap != null)
        {
            State.Icon = new MemoryStream();
            EntrySpecificationsViewModel.IconBitmap.Save(State.Icon);
        }
        else
        {
            State.Icon = null;
        }
    }
}