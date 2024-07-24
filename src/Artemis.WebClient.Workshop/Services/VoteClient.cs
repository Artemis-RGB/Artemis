using StrawberryShake;

namespace Artemis.WebClient.Workshop.Services;

public class VoteClient : IVoteClient
{
    private readonly Dictionary<long, bool> _cache = new();
    private readonly IWorkshopClient _client;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private DateTime _cacheAge = DateTime.MinValue;

    public VoteClient(IWorkshopClient client, IAuthenticationService authenticationService)
    {
        _client = client;
        authenticationService.IsLoggedIn.Subscribe(_ => _cacheAge = DateTime.MinValue);
    }

    /// <inheritdoc/>
    public async Task<bool?> GetVote(long entryId)
    {
        await _lock.WaitAsync();

        try
        {
            if (_cacheAge < DateTime.UtcNow.AddMinutes(-15))
            {
                _cache.Clear();
                IOperationResult<IGetVotesResult> result = await _client.GetVotes.ExecuteAsync();
                if (result.Data?.Votes != null)
                    foreach (IGetVotes_Votes vote in result.Data.Votes)
                        _cache.Add(vote.EntryId, vote.Upvote);
                _cacheAge = DateTime.UtcNow;
            }

            return _cache.TryGetValue(entryId, out bool upvote) ? upvote : null;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<IVoteCount?> CastVote(long entryId, bool upvote)
    {
        await _lock.WaitAsync();

        try
        {
            IOperationResult<ICastVoteResult> result = await _client.CastVote.ExecuteAsync(new CastVoteInput {EntryId = entryId, Upvote = upvote});
            if (result.IsSuccessResult() && result.Data?.CastVote != null)
                _cache[entryId] = upvote;

            return result.Data?.CastVote;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<IVoteCount?> ClearVote(long entryId)
    {
        await _lock.WaitAsync();

        try
        {
            IOperationResult<IClearVoteResult> result = await _client.ClearVote.ExecuteAsync(entryId);
            if (result.IsSuccessResult() && result.Data?.ClearVote != null)
                _cache.Remove(entryId);

            return result.Data?.ClearVote;
        }
        finally
        {
            _lock.Release();
        }
    }
}

public interface IVoteClient
{
    /// <summary>
    /// Gets the vote status for a specific entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry</param>
    /// <returns>A Task containing the vote status.</returns>
    Task<bool?> GetVote(long entryId);

    /// <summary>
    /// Casts a vote for a specific entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry.</param>
    /// <param name="upvote">A boolean indicating whether the vote is an upvote.</param>
    /// <returns>A Task containing the cast vote.</returns>
    Task<IVoteCount?> CastVote(long entryId, bool upvote);

    /// <summary>
    /// Clears a vote for a specific entry.
    /// </summary>
    /// <param name="entryId">The ID of the entry</param>
    /// <returns>A Task containing the vote status.</returns>
    Task<IVoteCount?> ClearVote(long entryId);
}