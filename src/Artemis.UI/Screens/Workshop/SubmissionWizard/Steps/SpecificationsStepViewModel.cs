using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.Entries;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;
using Artemis.WebClient.Workshop;
using DynamicData;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class SpecificationsStepViewModel : SubmissionViewModel
{
    private readonly Func<EntrySpecificationsViewModel> _getEntrySpecificationsViewModel;
    [Notify] private EntrySpecificationsViewModel? _entrySpecificationsViewModel;

    public SpecificationsStepViewModel(Func<EntrySpecificationsViewModel> getEntrySpecificationsViewModel)
    {
        _getEntrySpecificationsViewModel = getEntrySpecificationsViewModel;

        GoBack = ReactiveCommand.Create(ExecuteGoBack);
        this.WhenActivated((CompositeDisposable d) =>
        {
            DisplayName = $"{State.EntryType} Information";
            ApplyFromState();
        });
    }

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
        if (EntrySpecificationsViewModel == null || !EntrySpecificationsViewModel.ValidationContext.GetIsValid())
            return;

        ApplyToState();
        State.ChangeScreen<SubmitStepViewModel>();
    }

    private void ApplyFromState()
    {
        EntrySpecificationsViewModel viewModel = _getEntrySpecificationsViewModel();

        // Basic fields
        viewModel.Name = State.Name;
        viewModel.Summary = State.Summary;
        viewModel.Description = State.Description;

        // Tags
        viewModel.Tags.Clear();
        viewModel.Tags.AddRange(State.Tags);

        // Categories
        viewModel.PreselectedCategories = State.Categories;

        // Icon
        if (State.Icon != null)
        {
            State.Icon.Seek(0, SeekOrigin.Begin);
            viewModel.IconBitmap = BitmapExtensions.LoadAndResize(State.Icon, 128);
        }

        EntrySpecificationsViewModel = viewModel;
        Continue = ReactiveCommand.Create(ExecuteContinue, EntrySpecificationsViewModel.ValidationContext.Valid);
    }

    private void ApplyToState()
    {
        if (EntrySpecificationsViewModel == null)
            return;

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