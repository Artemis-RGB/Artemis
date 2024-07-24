using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Entries;

public partial class EntryVoteViewModel : ActivatableViewModelBase
{
    private readonly IEntrySummary _entry;
    private readonly INotificationService _notificationService;
    private readonly IVoteClient _voteClient;
    private bool _voting;

    [Notify] private int _score;
    [Notify] private bool _upvoted;
    [Notify] private bool _downvoted;

    public EntryVoteViewModel(IEntrySummary entry, IAuthenticationService authenticationService, INotificationService notificationService, IVoteClient voteClient)
    {
        _entry = entry;
        _notificationService = notificationService;
        _voteClient = voteClient;

        IsLoggedIn = authenticationService.IsLoggedIn;
        Score = entry.UpvoteCount - entry.DownvoteCount;
        this.WhenActivated(d => IsLoggedIn.Subscribe(l => _ = GetVoteStatus(l)).DisposeWith(d));
    }

    public IObservable<bool> IsLoggedIn { get; }

    public async Task CastVote(bool upvote)
    {
        // Could use a ReactiveCommand to achieve the same thing but that disables the button
        // while executing which grays it out for a fraction of a second and looks bad
        if (_voting)
            return;

        _voting = true;
        try
        {
            IVoteCount? result;
            // If the vote was removed, reset the upvote/downvote state
            if ((Upvoted && upvote) || (Downvoted && !upvote))
            {
                result = await _voteClient.ClearVote(_entry.Id);
                Upvoted = false;
                Downvoted = false;
            }
            else
            {
                result = await _voteClient.CastVote(_entry.Id, upvote);
                Upvoted = upvote;
                Downvoted = !upvote;
            }

            if (result != null)
                Score = result.UpvoteCount - result.DownvoteCount;
            else
                _notificationService.CreateNotification().WithTitle("Failed to cast vote").WithMessage("Please try again later.").WithSeverity(NotificationSeverity.Error).Show();
        }
        finally
        {
            _voting = false;
        }
    }

    private async Task GetVoteStatus(bool isLoggedIn)
    {
        if (!isLoggedIn)
        {
            Upvoted = false;
            Downvoted = false;
        }
        else
        {
            bool? vote = await _voteClient.GetVote(_entry.Id);
            if (vote != null)
            {
                Upvoted = vote.Value;
                Downvoted = !vote.Value;
            }
        }
    }
}