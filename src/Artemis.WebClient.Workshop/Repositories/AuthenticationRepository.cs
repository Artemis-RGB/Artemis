using Artemis.WebClient.Workshop.Entities;
using LiteDB;

namespace Artemis.WebClient.Workshop.Repositories;

internal class AuthenticationRepository : IAuthenticationRepository
{
    private readonly LiteRepository _repository;

    public AuthenticationRepository(LiteRepository repository)
    {
        _repository = repository;
        _repository.Database.GetCollection<RefreshTokenEntity>().EnsureIndex(s => s.RefreshToken);
    }

    /// <inheritdoc />
    public void SetRefreshToken(string? refreshToken)
    {
        _repository.Database.GetCollection<RefreshTokenEntity>().DeleteAll();

        if (refreshToken != null)
            _repository.Insert(new RefreshTokenEntity {RefreshToken = refreshToken});
    }

    /// <inheritdoc />
    public string? GetRefreshToken()
    {
        return _repository.Query<RefreshTokenEntity>().FirstOrDefault()?.RefreshToken;
    }
}

internal interface IAuthenticationRepository
{
    void SetRefreshToken(string? refreshToken);
    string? GetRefreshToken();
}