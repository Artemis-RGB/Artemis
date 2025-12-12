using System.ComponentModel.DataAnnotations;
using System.Reactive.Disposables;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Layout;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Plugin;
using Artemis.UI.Screens.Workshop.SubmissionWizard.Steps.Profile;
using Artemis.WebClient.Workshop;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.SubmissionWizard.Steps;

public partial class ChangelogStepViewModel : SubmissionViewModel
{
    [Notify] private string _changelog = string.Empty;

    public ChangelogStepViewModel()
    {
        GoBack = ReactiveCommand.Create(ExecuteGoBack);
        Continue = ReactiveCommand.Create(ExecuteContinue);
        ContinueText = "Submit";

        this.WhenActivated((CompositeDisposable _) => Changelog = State.Changelog ?? string.Empty);
    }

    private void ExecuteContinue()
    {
        State.Changelog = Changelog;
        State.ChangeScreen<UploadStepViewModel>();
    }

    private void ExecuteGoBack()
    {
        State.Changelog = string.IsNullOrWhiteSpace(Changelog) ? null : Changelog;
        if (State.EntryType == EntryType.Layout)
            State.ChangeScreen<LayoutInfoStepViewModel>();
        else if (State.EntryType == EntryType.Plugin)
            State.ChangeScreen<PluginSelectionStepViewModel>();
        else if (State.EntryType == EntryType.Profile)
            State.ChangeScreen<ProfileAdaptionHintsStepViewModel>();
    }
}